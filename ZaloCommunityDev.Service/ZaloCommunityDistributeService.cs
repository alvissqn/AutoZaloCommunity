using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using Managed.Adb;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service
{
    public class ZaloCommunityDistributeService
    {
        private ILog log = LogManager.GetLogger(nameof(ZaloCommunityDistributeService));

        public enum Activity
        {
            UserNearbySettings,
            UserNearbyList
        }

        public static string AssemblyPath = Assembly.GetEntryAssembly().Location;

        private AndroidDebugBridge _adb;
        private string _adbPath;
        private Device _device;
        private ConsoleOutputReceiver _receiver;

        private int currentaction;
        private int delay = 1000;
        private int delaynet;
        private int NoImg;

        private Settings _settings;

        private bool IsDebug => _settings.IsDebug;

        private void InvokeProc(string args)
        {
            var _process = new Process();
            _process.StartInfo.UseShellExecute = true;
            _process.StartInfo.WorkingDirectory = _adbPath;
            _process.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            _process.StartInfo.Arguments = args;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.Start();
        }
        IZaloImageProcessing _zaloImageProcessing;
        DatabaseContext _dbContext;
        public ZaloCommunityDistributeService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));


            if (string.IsNullOrWhiteSpace(settings.AndroidDebugBridgeOsLocation))
                throw new ArgumentNullException("Must declare AndroidDebugBridge Os Location");


            _zaloImageProcessing = zaloImageProcessing;

            _settings = settings;
            _dbContext = dbContext;

            _receiver = new ConsoleOutputReceiver();
            _adbPath = settings.AndroidDebugBridgeOsLocation;
            try
            {
                _adb = AndroidDebugBridge.CreateBridge(Path.Combine(_settings.AndroidDebugBridgeOsLocation, "adb.exe"), true);
                _adb.Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void StartAvd(string serialNumberOrIndex)
        {
            try
            {
                int value = 0;
                if (int.TryParse(serialNumberOrIndex, out value))
                {
                    _device = _adb.Devices.FirstOrDefault(x => x.IsOnline);
                }
                else
                {
                    _device = _adb.Devices.First(x => x.SerialNumber == serialNumberOrIndex && x.IsOnline);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void GotoActivity(Activity activity)
        {
            const string ActivityStart = "/c adb shell am start -n";
            string arguments;

            switch (activity)
            {
                case Activity.UserNearbyList:
                    arguments = $@"{ActivityStart} com.zing.zalo/.ui.UserNearbyListActivity";

                    break;
                case Activity.UserNearbySettings:
                    arguments = $@"{ActivityStart} com.zing.zalo/.ui.UserNearbySettingsActivity";

                    break;

                default:
                    return;

            }

            Delay(_settings.Delay.BetweenActivity);

            InvokeProc(arguments);

            Delay(_settings.Delay.BetweenActivity);

        }

        private void ScrollList(int times)
        {
            TouchSwipe(Screen.WorkingRect.Center.X, Screen.WorkingRect.Center.Y, Screen.WorkingRect.Center.X, Screen.WorkingRect.Center.Y - Screen.FriendRowHeight, times);
        }

        public void AddFriendNearBy(Filter filter)
        {
            var gender = filter.GenderSelection;
            var ageValues = filter.FilterAgeRange.Split("-".ToArray());
            var age_from = ageValues[0];
            var age_to = ageValues[1];
            var numFriends = filter.NumberOfAction;

            GotoActivity(Activity.UserNearbyList);

            ConfigsSearchFriend(gender, age_from, age_to);
            //I'm on Search Page
            TouchAt(Screen.IconTopRight);//Open Right SideBar
            Delay(500);
            TouchGenderOnSideBar(gender);

            int maxFriendToday = _settings.MaxFriendAddedPerDay - _settings.AddedFriendTodayCount;

            if (maxFriendToday > numFriends)
                maxFriendToday = numFriends;

            if (maxFriendToday < 0)
                maxFriendToday = 0;

            AddFriend(maxFriendToday, filter);
        }

        private void AddFriend(int maxFriendToday, Filter filter)
        {
            Console.WriteLine($"!bắt đầu thêm bạn. số bạn yêu cầu tối đa trong ngày hôm nay là {maxFriendToday}");
            bool finish = false;

            int countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            Console.WriteLine("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotAdded((x) => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
            var points = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach((x)=> Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
            Console.WriteLine($"!--------------------");
            friendNotAdded.ToList().ForEach((x) => Console.WriteLine($"!các bạn chưa được gửi lời mời: {x}"));
            while (!finish)
            {

                while (points.Count == 0)
                {
                    Console.WriteLine("!đang cuộn danh sách bạn");
                    ScrollList(9);

                    Console.WriteLine("!đang tìm thông tin các bạn");

                    friendNotAdded = (GetPositionAccountNotAdded((x) => profilesPage2 = x)).OrderByDescending(x => x.Point.Y);
                    points = new Stack<FriendPositionMessage>(friendNotAdded);

                    profilesPage1.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
                    Console.WriteLine($"!--------------------");
                    friendNotAdded.ToList().ForEach((x) => Console.WriteLine($"!các bạn chưa được gửi lời mời: {x}"));

                    profilesPage2.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));

                    if (profilesPage2.Except(profilesPage1).Count() == 0)
                    {
                        Console.WriteLine("!hết bạn trong danh sách.");
                        return;
                    }

                    profilesPage1 = profilesPage2;

                }

                Delay(2000);

                var pointRowFriend = points.Pop();

                
                ProfileMessage profile = new ProfileMessage() { Name = pointRowFriend.Name };
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToAddFriendAt(profile, pointRowFriend.Point, filter))
                {
                    _dbContext.AddProfile(profile);
                    //Add Log
                    countSuccess++;
                    Console.WriteLine($"!yêu cầu kết bạn [{countSuccess}]: {profile.Name} bị thành công.");
                }

                finish = countSuccess == maxFriendToday;
            }
        }

        private FriendPositionMessage[] GetPositionAccountNotAdded(Action<string[]> allPrrofiles)
        {
            var captureFiles = CaptureScreenNow();
            var names = _zaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => _dbContext.ProfileSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t.ToArray();
        }

        ScreenInfo Screen => _settings.Screen;


        public bool ClickToAddFriendAtRowPosition(ProfileMessage profile, int position, Filter filter)
        {
            return ClickToAddFriendAt(profile, Screen.MenuPoint.Y * 2, (position - 1) * Screen.FriendRowHeight + Screen.FriendRowHeight / 2 + Screen.HeaderHeight, filter);
        }

        public bool ClickToAddFriendAt(ProfileMessage profile, ScreenPoint point, Filter filter)
        {
            return ClickToAddFriendAt(profile, point.X, point.Y, filter);
        }

        public bool ClickToAddFriendAt(ProfileMessage profile, int x, int y, Filter filter)
        {
            Console.WriteLine($"!đã nhấn vào bạn: {profile.Name}");

            TouchAt(x, y);//TOUCH TO ROW_INDEX

            Delay(2000);
            //I''m on profile page

            var info = GrabProfileInfo(profile.Name);
            CopyProfile(profile, info);

            if (!info.IsAddedToFriend)
            {
                Console.WriteLine($"!tiến hành gửi yêu cầu kết bạn: {profile.Name}");
                //Wait to navigate to profiles
                TouchAtIconBottomRight();//Touch to AddFriends
                                         //Wait to Navigate to new windows
                Delay(3000);

                var proccessedDialog = ProcessIfShowDialogWaitRequestedFriendConfirm();
                if (proccessedDialog)
                {
                    Console.WriteLine($"!yêu cầu kết bạn: {profile.Name} bị từ chối. Lý do: đã gửi yêu cầu rồi");
                    TouchAtIconTopLeft(); //GoBack to friendList
                    return false;
                }

                TouchAt(Screen.AddFriendScreenGreetingTextField);

                var textGreeting = info.Gender == "Nam" ? filter.TextGreetingForMale : filter.TextGreetingForFemale;

                SendText(textGreeting);

                //Debug:
                TouchAtIconTopLeft();
                //TouchAt(Screen.AddFriendScreenOkButton); //TouchToAddFriend then goto profile

                Delay(300);

                TouchAtIconTopLeft(); //GoBack to friendList

                return true;
            }
            else
            {
                Console.WriteLine($"!yêu cầu kết bạn: {profile.Name} bị từ hủy. Lý do: đã có tên trong cơ sở dữ liệu");
                TouchAtIconTopLeft(); //GoBack to friendList
                return false;
            }
        }

        private static void CopyProfile(ProfileMessage profile, ProfileMessage info)
        {
            profile.BirthdayText = info.BirthdayText;
            profile.Gender = info.Gender;
            profile.IsAddedToFriend = info.IsAddedToFriend;
            profile.Name = string.IsNullOrWhiteSpace(profile.Name) ? info.Name : profile.Name;
            profile.PhoneNumber = info.PhoneNumber;
        }

        private ProfileMessage GrabProfileInfo(string initName = null)
        {
            Console.WriteLine($"!đang lấy thông tin bạn: {initName}");

            TouchAt(Screen.ProfileScreenTabInfo); //Touch tab Thong Tin
            Delay(300);

            var file = CaptureScreenNow();
            var profile = _zaloImageProcessing.GetProfile(file, Screen);
            if (!string.IsNullOrWhiteSpace(initName))
            {
                profile.Name = initName;
            }
            Console.WriteLine($"!@: {profile.Name} {profile.BirthdayText} {profile.Gender} {profile.PhoneNumber}");
            return profile;
        }

        private bool ProcessIfShowDialogWaitRequestedFriendConfirm()
        {
            var file = CaptureScreenNow();
            var hasDialog = _zaloImageProcessing.IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(file, Screen);
            if (hasDialog)
            {
                TouchAt(Screen.AddFriendScreenWaitFriendConfirmDialog);

                return true;
            }

            return false;
        }

        public void ConfigsSearchFriend(GenderSelection gender, string age_from, string age_to)
        {
            GotoActivity(Activity.UserNearbySettings);

            TouchGender(gender);
            TouchAgeRange(age_from, age_to);
            Delay(300);
            TouchAt(Screen.ConfigSearchFriendUpdateButton);//TOUCH Update
        }

        private void TouchAgeRange(string age_from, string age_to)
        {
            TouchAt(Screen.ConfigSearchFriendAgeCombobox);//touch to do tuoi
            Delay(300);

            TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//touch to age to
            SendText(age_from);

            TouchAt(Screen.ConfigSearchFriendAgeToTextField);//Touch to age from
            SendText(age_to);

            TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//re-touch to age to

            Delay(200);

            TouchAt(Screen.ConfigSearchFriendAgeOkButton); //touch OK
            Delay(300);
        }

        private void TouchGender(GenderSelection gender)
        {
            TouchAt(Screen.ConfigSearchFriendGenderCombobox);
            Delay(200);
            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    TouchAt(Screen.ConfigSearchFriendMaleOnlyComboboxItem);
                    break;

                case GenderSelection.OnlyFemale:
                    TouchAt(Screen.ConfigSearchFriendFemaleOnlyComboboxItem);
                    break;

                default:
                    TouchAt(Screen.ConfigSearchFriendBothGenderComboboxItem);
                    break;
            }
        }

        private void TouchGenderOnSideBar(GenderSelection gender)
        {
            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    TouchAt(Screen.SearchFriendSideBarMaleOnlyTextItem);
                    break;

                case GenderSelection.OnlyFemale:
                    TouchAt(Screen.SearchFriendSideBarFemaleOnlyTextItem);
                    break;

                default:
                    TouchAt(Screen.SearchFriendSideBarBothGenderTextItem);
                    break;
            }
        }


        public void AddFriendPhone(List<string> phonelist, int numfriends, string text)
        {
            numfriends = (numfriends < phonelist.Count) ? numfriends : phonelist.Count;
            int num = 0;
            var total_action = 0;
            while ((num < numfriends))
            {
                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");
                Thread.Sleep(delay);
                SendText(phonelist[total_action].ToString());
                SendKey(KeyCode.AkeycodeEnter, 1);
                Thread.Sleep(delaynet);
                TouchAt(0x2fd, 70, 1);
                TouchAt(780, 200, 2);
                SendText(text);

                TouchAt(360, 340, 1);

                num++;
                total_action++;
            }
            TouchAt(100, 0x4b, 5);
        }

        public void ChangeDes(int des)
        {
            if (_device == null || _device.State == DeviceState.Online)
            {
                log.Error("Device null");
                return;
            }

            _device.ExecuteShellCommand("adb root" + des.ToString(), _receiver);
            _device.ExecuteShellCommand("adb shell am display-density " + des.ToString(), _receiver);
        }

        public void ChangeSize(int x, int y)
        {
            if (_device == null || _device.State == DeviceState.Online)
            {
                log.Error("Device null");
                return;
            }

            _device.ExecuteShellCommand("adb root", _receiver);
            _device.ExecuteShellCommand("adb shell am display-size " + x.ToString() + "x" + y.ToString(), _receiver);
        }

        public void SpamFriend(Filter config)
        {
            bool finish = false;

            int countSuccess = 0;

            string[] profilesPage1 = null;
            string[] profilesPage2 = null;

            var fileCapture = CaptureScreenNow();
            var friends = _zaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
            var points = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
            profilesPage1 = points.Select(x => x.Name).ToArray();

            while (!finish)
            {

                while (points.Count == 0)
                {
                    ScrollList(9);

                    fileCapture = CaptureScreenNow();
                    friends = _zaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                    points = new Stack<FriendPositionMessage>(friends.OrderByDescending(x => x.Point.Y));
                    profilesPage2 = points.Select(x => x.Name).ToArray();
                    if (profilesPage2.Except(profilesPage1).Count() == 0)
                    {
                        //add het ban
                        return;
                    }

                    profilesPage1 = profilesPage2;
                }

                Delay(2000);

                var pointRowFriend = points.Pop();
                ProfileMessage profile = new ProfileMessage();
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToChatFriendAt(profile, pointRowFriend.Point, config))
                {
                    //save
                    //Add Log
                    countSuccess++;
                }

                //finish = countSuccess == maxFriendToday;
            }
        }

        private bool ClickToChatFriendAt(ProfileMessage profile, ScreenPoint point, Filter config)
        {
            TouchAt(point);
            Delay(2000);//wait to navigate chat screen

            //GrabInfomation
            TouchAtIconTopRight();
            Delay(200);
            TouchAt(Screen.ChatScreenProfileAvartar);
            Delay(200);

            var infoGrab = GrabProfileInfo(profile.Name);
            CopyProfile(profile, infoGrab);

            TouchAtIconTopLeft();//Back to chat screen
            TouchAtIconTopLeft();//Close sidebar
            //End friend
            TouchAt(Screen.ChatScreenTextField);
            Delay(300);
            SendText(config.TextGreetingForFemale);
            Delay(500);
            //TouchAt(Screen.ChatScreenSendButton);

            Delay(1000);

            TouchAt(Screen.IconTopLeft);//Go Back

            //_dbContext.LogSpamFriend(friendName);

            return true;
        }

        public void SpamFriend2(Filter config)
        {
            Thread.Sleep((int)(delaynet + delay));
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MainTabActivity");

            bool finish = false;
            while (!finish)
            {
                var fileCapture = CaptureScreenNow();
                var points = _zaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                var stack = new Stack<FriendPositionMessage>(points.OrderByDescending(x => x.Point.Y));

                if (stack.Count() == 0)
                {

                }

                if (stack.Count() > 0)
                {
                    var pos = stack.Pop();

                    TouchAt(pos.Point);
                    Delay(2000);//wait to navigate chat screen

                    TouchAt(Screen.ChatScreenTextField);
                    Delay(300);
                    SendText(config.TextGreetingForFemale);
                    Delay(500);
                    //TouchAt(Screen.ChatScreenSendButton);

                    Delay(1000);

                    TouchAt(Screen.IconTopLeft);//Go Back
                }
            }
        }

        public void ChatAllfriends(string text, string path)
        {

            Thread.Sleep((int)(delaynet + delay));
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MainTabActivity");

            TouchAt(0x159, 0x4b, 2);
            Bitmap bitmap = new Bitmap(Screencapture("chatfr").FullName);
            new List<string>();
            int num = 0;
            for (int i = 600; i < 0x4b0; i++)
            {
                if (HexConverter(bitmap.GetPixel(0x2ac, i)) == "#158FC2")
                {
                    return;
                }
            }
            string str = HexConverter(bitmap.GetPixel(0x2ac, 0x457));
            bitmap.Dispose();
            if (str != "#158FC2")
            {
                UpImageChat(path);
                Thread.Sleep(delay);
                SendKey(KeyCode.AkeycodeDpadDown, 1);
                while (str != "#158FC2")
                {
                    for (int j = 0; j < 10; j++)
                    {

                        SendKey(KeyCode.AkeycodeDpadDown, 2);
                        SendKey(KeyCode.AkeycodeEnter, 1);
                        Thread.Sleep(delay);
                        TouchAt(200, 0x492, 1);
                        string str2 = ResultSpin(text);
                        if (path.Length != 0)
                        {
                            SendText(str2);
                            if (path.Length != 0)
                            {
                                TouchAt(0x2ff, 0x474, 1);
                                Thread.Sleep(300);
                                SendText("/mnt/sdcard/DCIM/image.png");
                                Thread.Sleep(300);
                                TouchAt(0x2ff, 0x474, 1);
                            }
                            else
                            {
                                Thread.Sleep(delay);
                                TouchAt(0x2ff, 0x474, 1);
                            }
                        }
                        else
                        {
                            SendText(str2);
                            Thread.Sleep(delay);
                            TouchAt(0x2ff, 0x474, 1);
                        }
                        TouchAt(100, 0x4b, 1);
                    }
                    bitmap = new Bitmap(Screencapture("chatfr").FullName);
                    str = HexConverter(bitmap.GetPixel(0x2ac, 0x457));
                    num++;
                    bitmap.Dispose();
                }
            }
            TouchAt(100, 0x4b, 3);
        }

        public void Chatfriendnearby(string gender, string age_from, string age_to, int numFriends, string text, string path)
        {
            GotoActivity(Activity.UserNearbyList);


            Thread.Sleep(delay);
            UpImageChat(path);
            TouchAt(610, 310, 1);
            TouchAt(340, 620, 1);
            SendText(age_from);
            TouchAt(460, 620, 1);
            SendText(age_to);
            TouchAt(500, 830, 1);
            TouchAt(400, 410, 1);
            Thread.Sleep((int)(delaynet + delay));
            TouchAt(760, 0x48, 1);
            switch (gender)
            {
                case "Nam":
                    TouchAt(440, 0x84, 1);
                    break;

                case "Nữ":
                    TouchAt(440, 0x42, 1);
                    break;

                default:
                    TouchAt(440, 0xd4, 1);
                    break;
            }
            for (int i = 0; (i < numFriends); i++)
            {
                Thread.Sleep(delay);
                SendKey(KeyCode.AkeycodeDpadDown, 2);
                SendKey(KeyCode.AkeycodeEnter, 1);
                Thread.Sleep(delaynet);
                TouchAt(40, 0x492, 1);
                TouchAt(200, 0x492, 1);
                string str = ResultSpin(text);
                SendText(str);
                if (path.Length != 0)
                {
                    TouchAt(0x2ff, 0x474, 1);
                    Thread.Sleep(500);
                    SendText("/mnt/sdcard/DCIM/image.png");
                    Thread.Sleep(500);
                    TouchAt(0x2ff, 0x474, 1);
                }
                else
                {
                    TouchAt(0x2ff, 0x474, 1);
                }
                SendKey(KeyCode.AkeycodeBack, 2);
            }
            TouchAt(100, 0x4b, 3);
        }

        public void Chatfriendphone(List<string> phonelist, int numfriends, string text, string path)
        {
            Thread.Sleep((int)(delaynet + delay));
            numfriends = (numfriends < phonelist.Count) ? numfriends : phonelist.Count;
            int num = 0;
            UpImageChat(path);
            var total_action = 0;
            while ((num < numfriends))
            {

                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");

                Thread.Sleep(delaynet);
                SendText(phonelist[total_action].ToString());
                SendKey(KeyCode.AkeycodeEnter, 1);
                Thread.Sleep(delaynet);
                TouchAt(40, 0x492, 1);
                TouchAt(200, 0x492, 1);
                string str = ResultSpin(text);
                SendText(str);
                if (path.Length != 0)
                {
                    TouchAt(0x2ff, 0x492, 1);
                    Thread.Sleep(500);
                    SendText("/mnt/sdcard/DCIM/image.png");
                    Thread.Sleep(500);
                    TouchAt(0x2ff, 0x492, 1);
                }
                else
                {
                    Thread.Sleep(delay);
                    TouchAt(0x2ff, 0x492, 1);
                }
                TouchAt(100, 0x4b, 1);
                num++;
                currentaction = num;
                total_action++;
            }
        }

        public void DelImagePost()
        {
            try
            {
                InvokeProc("/c adb shell rm -f sdcard/DCIM/*.*");
                Thread.Sleep(delay);
            }
            catch
            {
            }
            Thread.Sleep(0x3e8);
        }

        public void EnableAbdKeyoard()
        {
            InvokeProc("/c adb shell ime set com.android.adbkeyboard/.AdbIME");
            Thread.Sleep(500);
        }

        public void SendKey(KeyCode keycode, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    _device.ExecuteShellCommand("input keyevent " + (int)keycode, _receiver);
                    Delay(_settings.Delay.PressedKeyEvent);
                }
            }
            else
            {
                log.Error("Can't send keyevent");

            }
        }

        public void Login(string account, string password, string region)
        {
            EnableAbdKeyoard();
            try
            {
                InvokeProc("/c adb shell am force-stop com.zing.zalo");

                Delay(_settings.Delay.WaitForceCloseApp);

                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.LoginUsingPWActivity");

                Delay(_settings.Delay.WaitLoginScreenOpened);

                //Open regions
                TouchAt(Screen.LoginScreenCountryCombobox);
                Delay((delaynet + delay));
                TouchAt(Screen.IconTopRight);
                SendText(region);
                TouchAt(Screen.LoginScreenFirstCountryItem);
                Delay(100);

                //Enter username
                TouchAt(Screen.LoginScreenPhoneTextField);
                for (int i = 0; i < 12; i++)
                {
                    SendKey(KeyCode.AkeycodeDel);
                }
                SendText(account);

                //Enter Password
                Delay(100);
                TouchAt(Screen.LoginScreenPasswordTextField);
                SendText(password);
                TouchAt(Screen.LoginScreenOkButton);
                //SendKey(KeyCode.AkeycodeEnter, 2);

                Delay(_settings.Delay.WaitLogin);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void Post(string text, string path)
        {
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MyInfoActivity");

            Thread.Sleep((int)(delay + delaynet));
            TouchAt(0x2ff, 0x492, 1);
            Thread.Sleep(delay);
            if (path.Length != 0)
            {
                TouchAt(110, 320, 1);
                TouchAt(250, 600, 1);
                Thread.Sleep(delay);
                TouchAt(200, 200, 1);
                //if ((mainForm.pack == "basic") || (mainForm.pack == "free"))
                //{
                //    Touch(230, 0x9b, 1);
                //    Touch(500, 0x9b, 1);
                //}
                //else 
                if (NoImg > 4)
                {
                    TouchAt(230, 0x9b, 1);
                    TouchAt(500, 0x9b, 1);
                    TouchAt(0x2fd, 0x9b, 1);
                    TouchAt(230, 420, 1);
                    TouchAt(500, 420, 1);
                    TouchAt(0x2fd, 420, 1);
                    TouchAt(230, 0x2ad, 1);
                    TouchAt(500, 0x2ad, 1);
                    TouchAt(0x2fd, 0x2ad, 1);
                }
                else
                {
                    TouchAt(230, 0x9b, 1);
                    TouchAt(500, 0x9b, 1);
                    TouchAt(0x2fd, 0x9b, 1);
                    TouchAt(230, 420, 1);
                }
                TouchAt(720, 0x492, 1);
                Thread.Sleep(delay);
                TouchAt(700, 150, 1);
                SendText(text);
                TouchAt(0x2ff, 0x45, 1);
            }
            else
            {
                Thread.Sleep(delay);
                TouchAt(700, 150, 1);
                SendText(text);
                TouchAt(0x2ff, 0x45, 1);
            }
        }

        public string ResultSpin(string str)
        {
            Random rnd = new Random();
            return Spintext(rnd, str);
        }

        public string CaptureScreenNow()
        {
            var fileName = DateTime.Now.Ticks.ToString();

            InvokeProc($"/c adb shell screencap -p sdcard/DCIM/{fileName}.png");

            Delay(3000);

            InvokeProc($"/c adb pull sdcard/DCIM/{fileName}.png d:/{fileName}.png");

            Delay(3000);

            return $@"d:\{fileName}.png";
        }

        public FileInfo Screencapture(string filename)
        {
            var tickName = DateTime.Now.Ticks.ToString();

            InvokeProc($"/c adb shell screencap -p sdcard/DCIM/{tickName}.png");

            Thread.Sleep(delay);

            InvokeProc("/c adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");

            InvokeProc($"/c adb pull sdcard/DCIM/{tickName}.png {AssemblyPath}/{filename}.png");

            Thread.Sleep(100);
            InvokeProc($"/c adb pull sdcard/DCIM/{tickName}.png {AssemblyPath}/{filename}.png");

            FileInfo info = new FileInfo($"{AssemblyPath}/{filename}.png");
            Thread.Sleep(delay);
            return info;
        }

        private IEnumerable<byte[]> SplitIntoChunks(byte[] value, int bufferLength)
        {
            int iteratorVariable0 = value.Length / bufferLength;
            if ((value.Length % bufferLength) > 0)
            {
                iteratorVariable0++;
            }
            for (int i = 0; i < iteratorVariable0; i++)
            {
                yield return value.Skip<byte>((i * bufferLength)).Take<byte>(bufferLength).ToArray<byte>();
            }
        }

        public void SendText(string text)
        {
            if (_device == null || _device.State != DeviceState.Online)
            {
                log.Error("Device null");

                return;
            }

            if (Encoding.UTF8.GetBytes(text).Length <= 0x3d0)
            {
                _device.ExecuteShellCommand("am broadcast -a ADB_INPUT_TEXT --es msg '" + text + "'", _receiver);
            }
            else
            {
                foreach (byte[] buffer2 in SplitIntoChunks(Encoding.UTF8.GetBytes(text), 0x3d0))
                {
                    _device.ExecuteShellCommand("am broadcast -a ADB_INPUT_TEXT --es msg '" + Encoding.UTF8.GetString(buffer2) + "'", _receiver);
                }
            }

            Delay(_settings.Delay.PressedKeyEvent);
        }

        public void SetGPS(string lat, string longt)
        {
            InvokeProc("/c adb shell am force-stop com.cxdeberry.geotag");

            Delay(_settings.Delay.CloseMap);


            InvokeProc("/c adb shell am start -n com.cxdeberry.geotag/.MainActivity");

            Delay(_settings.Delay.OpenMap);

            TouchAt(750, 50, 1);

            SendText(lat + "," + longt);
            SendKey(KeyCode.AkeycodeEnter);

            TouchAt(750, 50, 1);

            Delay(_settings.Delay.CloseMap);

            TouchAt(200, 0x438, 1);
        }

        public void setKeyBoardTelex(bool check)
        {
            if (_device == null || _device.State == DeviceState.Online)
            {
                log.Error("Device null");
                return;
            }

            _device.ExecuteShellCommand("adb shell ime set com.android.adbkeyboard/.AdbIME", _receiver);
            Thread.Sleep(500);
        }


        private static string Spintext(Random rnd, string str)
        {
            string pattern = "{[^{}]*}";
            for (Match match = Regex.Match(str, pattern); match.Success; match = Regex.Match(str, pattern))
            {
                string[] strArray = str.Substring(match.Index + 1, match.Length - 2).Split(new char[] { '|' });
                str = str.Substring(0, match.Index) + strArray[rnd.Next(strArray.Length)] + str.Substring(match.Index + match.Length);
            }
            return str;
        }

        public void StopADB()
        {
            _adb.Stop();
        }


        #region Touches
        public void TouchAtIconTopLeft()
        {
            TouchAt(Screen.IconTopLeft);
        }

        public void TouchAtIconTopRight()
        {
            TouchAt(Screen.IconTopRight);
        }

        public void TouchAtIconBottomLeft()
        {
            TouchAt(Screen.IconBottomLeft);
        }

        public void TouchAtIconBottomRight()
        {
            TouchAt(Screen.IconBottomRight);
        }

        public void TouchAt(ScreenPoint point, int times = 1)
        {
            TouchAt(point.X, point.Y, times);
        }

        public void TouchSwipe(ScreenPoint point1, ScreenPoint point2, int times = 1)
        {
            TouchSwipe(point1.X, point1.Y, point2.X, point2.Y, times);
        }

        public void TouchAt(int x, int y, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    _device.ExecuteShellCommand($"input tap {x} {y}", _receiver);
                    Delay(_settings.Delay.TouchEvent);
                }
            }
            else
            {
                log.Error("Can't touch");
            }
        }

        public void TouchSwipe(int x1, int y1, int x2, int y2, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    InvokeProc($"/c adb shell input swipe {x1} {y1} {x2} {y2}");
                    Delay(_settings.Delay.ScrollEvent);
                }
            }
            else
            {
                log.Error("Can't touch swipe");
            }
        }
        #endregion

        public void Delay(int milisecond)
        {
            Thread.Sleep(milisecond);
        }

        public void UpImage(string path)
        {
            InvokeProc("/c adb push \"" + path + "\" sdcard/DCIM && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(300);
        }

        public void UpImageChat(string path)
        {
            InvokeProc("/c adb push \"" + path + "\" sdcard/DCIM/image.png && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(300);
        }

        public void UpImagePost(string path)
        {
            InvokeProc("/c adb push \"" + path + "\" sdcard/DCIM && adb shell am broadcast -a android.intent.action.MEDIA_MOUNTED -d file:///sdcard/DCIM/");
            Thread.Sleep(300);
        }

    }
}