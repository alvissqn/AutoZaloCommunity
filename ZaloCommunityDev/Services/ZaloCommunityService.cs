using GalaSoft.MvvmLight;
using log4net;
using Managed.Adb;
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
using System.Threading.Tasks;
using ZaloCommunityDev.DAL;
using ZaloCommunityDev.DAL.Models;
using ZaloCommunityDev.Models;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.ViewModel;
using ZaloImageProcessing207;
using ZaloImageProcessing207.Structures;

namespace ZaloCommunityDev.Services
{
    public class ZaloCommunityService
    {
        private ILog log = LogManager.GetLogger(nameof(ZaloCommunityService));

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
        private bool istop;
        private int NoImg;

        private SettingViewModel _settings;

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
        public ZaloCommunityService(SettingViewModel settings, IZaloImageProcessing zaloImageProcessing, DatabaseContext dbContext)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));


            if (string.IsNullOrWhiteSpace(settings.AndroidDebugBridgeOsLocation))
                throw new ArgumentNullException("Must declare AndroidDebugBridge Os Location");


            _zaloImageProcessing = zaloImageProcessing;
            this._dbContext = dbContext;

            _settings = settings;

            _receiver = new ConsoleOutputReceiver();
            _adbPath = settings.AndroidDebugBridgeOsLocation;

            istop = false;

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


        public string[] OnlineDevices => _adb?.Devices.Where(x => x.IsOnline).Select(x => x.SerialNumber).ToArray();

        public void StartAvd(string name)
        {
            try
            {

                _device = _adb.Devices.First(x => x.SerialNumber == name);
                while (_device.State != DeviceState.Online)
                {
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async Task GotoActivity(Activity activity)
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

            await Task.Delay(_settings.Delay.BetweenActivity);

            InvokeProc(arguments);

            await Task.Delay(_settings.Delay.BetweenActivity);

        }

        private async Task ScrollFriendList(int times)
        {
            await TouchSwipe(_settings.Screen.WorkingRect.Center.X, _settings.Screen.WorkingRect.Center.Y, _settings.Screen.WorkingRect.Center.X, _settings.Screen.WorkingRect.Center.Y - _settings.Screen.FriendRowHeight, times);
        }

        public async Task AddFriendNearBy(AddFriendNearByConfig config)
        {

            var gender = config.GenderSelection;
            var age_from = config.AgeRange.Split("-".ToArray())[0];
            var age_to = config.AgeRange.Split("-".ToArray())[1];
            var numFriends = config.WishAddedNumberFriendPerDay;

            await GotoActivity(Activity.UserNearbyList);

            //await ConfigsSearchFriend(gender, age_from, age_to);
            //I'm on Search Page
            await TouchAt(Screen.IconTopRight);//Open Right SideBar
            await Task.Delay(500);
            await TouchGenderOnSideBar(gender);

            int maxFriendToday = _settings.MaxFriendAddedPerDay - _dbContext.GetAddedFriendCount();

            if (maxFriendToday > numFriends)
                maxFriendToday = numFriends;

            if (maxFriendToday < 0)
                maxFriendToday = 0;

            await AddFriend(maxFriendToday, config);
        }

        private async Task AddFriend(int maxFriendToday, AddFriendNearByConfig config)
        {
            bool finish = false;

            int countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;

            var points = new Stack<FriendPositionMessage>((await GetPositionAccountNotAdded((x) => profilesPage1 = x)).OrderByDescending(x => x.Point.Y));

            while (!finish)
            {

                while (points.Count == 0)
                {

                    await ScrollFriendList(10);

                    points = new Stack<FriendPositionMessage>((await GetPositionAccountNotAdded((x) => profilesPage2 = x)).OrderByDescending(x => x.Point.Y));
                    if (profilesPage2.Except(profilesPage1).Count() == 0)
                    {
                        //add het ban
                        return;
                    }

                    profilesPage1 = profilesPage2;

                }

                await Task.Delay(2000);

                var pointRowFriend = points.Pop();
                ProfileMessage profile = new ProfileMessage();
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && await ClickToAddFriendAt(profile, pointRowFriend.Point, config))
                {
                    _dbContext.AddProfile(profile);
                    //Add Log
                    countSuccess++;
                }

                finish = countSuccess == maxFriendToday;
            }
        }

        private async Task<FriendPositionMessage[]> GetPositionAccountNotAdded(Action<string[]> allPrrofiles)
        {
            var captureFiles = await CaptureScreenNow();
            var names = _zaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => _dbContext.ProfileSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t.ToArray();
        }

        ScreenInfo Screen => _settings.Screen;


        public async Task<bool> ClickToAddFriendAtRowPosition(ProfileMessage profile, int position, AddFriendNearByConfig config)
        {
            return await ClickToAddFriendAt(profile, Screen.MenuPoint.Y * 2, (position - 1) * Screen.FriendRowHeight + Screen.FriendRowHeight / 2 + Screen.HeaderHeight, config);
        }

        public async Task<bool> ClickToAddFriendAt(ProfileMessage profile, ScreenPoint point, AddFriendNearByConfig config)
        {
            return await ClickToAddFriendAt(profile, point.X, point.Y, config);
        }

        public async Task<bool> ClickToAddFriendAt(ProfileMessage profile, int x, int y, AddFriendNearByConfig config)
        {
            await TouchAt(x, y);//TOUCH TO ROW_INDEX

            await Task.Delay(2000);
            //I''m on profile page

            var info = await GrabProfileInfo(profile.Name);
            CopyProfile(profile, info);

            if (!info.IsAddedToFriend)
            {
                //Wait to navigate to profiles
                await TouchAtIconBottomRight();//Touch to AddFriends
                                               //Wait to Navigate to new windows
                await Task.Delay(3000);

                var proccessedDialog = await ProcessIfShowDialogWaitRequestedFriendConfirm();
                if (proccessedDialog)
                {
                    await TouchAtIconTopLeft(); //GoBack to friendList
                    return false;
                }

                await TouchAt(Screen.AddFriendScreenGreetingTextField); //Touch tab Thong Tin


                var textGreeting = info.Gender == "Nam" ? config.TextGreetingForMale : config.TextGreetingForFemale;


                await SendText(textGreeting);

                //Debug:
                await TouchAtIconTopLeft();
                //await TouchAt(Screen.AddFriendScreenOkButton); //TouchToAddFriend then goto profile

                await Task.Delay(2000);

                await Task.Delay(300);

                await TouchAtIconTopLeft(); //GoBack to friendList

                return true;
            }
            else
            {
                await TouchAtIconTopLeft(); //GoBack to friendList
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

        private async Task<ProfileMessage> GrabProfileInfo(string initName = null)
        {
            await TouchAt(Screen.ProfileScreenTabInfo); //Touch tab Thong Tin
            await Task.Delay(300);

            var file = await CaptureScreenNow();
            var profile = _zaloImageProcessing.GetProfile(file, _settings.Screen);

            return profile;
        }

        private async Task<bool> ProcessIfShowDialogWaitRequestedFriendConfirm()
        {
            var file = await CaptureScreenNow();
            var hasDialog = _zaloImageProcessing.IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(file, _settings.Screen);
            if (hasDialog)
            {
                await TouchAt(Screen.AddFriendScreenWaitFriendConfirmDialog);

                return true;
            }

            return false;
        }

        public async Task ConfigsSearchFriend(GenderSelection gender, string age_from, string age_to)
        {
            await GotoActivity(Activity.UserNearbySettings);

            await TouchGender(gender);
            await TouchAgeRange(age_from, age_to);
            await Task.Delay(300);
            await TouchAt(Screen.ConfigSearchFriendUpdateButton);//TOUCH Update
        }

        private async Task TouchAgeRange(string age_from, string age_to)
        {
            await TouchAt(Screen.ConfigSearchFriendAgeCombobox);//touch to do tuoi
            await Task.Delay(300);

            await TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//touch to age to
            await SendText(age_from);

            await TouchAt(Screen.ConfigSearchFriendAgeToTextField);//Touch to age from
            await SendText(age_to);

            await TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//re-touch to age to

            await Task.Delay(200);

            await TouchAt(Screen.ConfigSearchFriendAgeOkButton); //touch OK
            await Task.Delay(300);
        }

        private async Task TouchGender(GenderSelection gender)
        {
            await TouchAt(Screen.ConfigSearchFriendGenderCombobox);
            await Task.Delay(200);
            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    await TouchAt(Screen.ConfigSearchFriendMaleOnlyComboboxItem);
                    break;

                case GenderSelection.OnlyFemale:
                    await TouchAt(Screen.ConfigSearchFriendFemaleOnlyComboboxItem);
                    break;

                default:
                    await TouchAt(Screen.ConfigSearchFriendBothGenderComboboxItem);
                    break;
            }
        }

        private async Task TouchGenderOnSideBar(GenderSelection gender)
        {
            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    await TouchAt(Screen.SearchFriendSideBarMaleOnlyTextItem);
                    break;

                case GenderSelection.OnlyFemale:
                    await TouchAt(Screen.SearchFriendSideBarFemaleOnlyTextItem);
                    break;

                default:
                    await TouchAt(Screen.SearchFriendSideBarBothGenderTextItem);
                    break;
            }
        }


        public async Task AddFriendPhone(List<string> phonelist, int numfriends, string text)
        {
            numfriends = (numfriends < phonelist.Count) ? numfriends : phonelist.Count;
            int num = 0;
            var total_action = 0;
            while (!istop && (num < numfriends))
            {
                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");
                Thread.Sleep(delay);
                await SendText(phonelist[total_action].ToString());
                await SendKey(KeyCode.AkeycodeEnter, 1);
                Thread.Sleep(delaynet);
                await TouchAt(0x2fd, 70, 1);
                await TouchAt(780, 200, 2);
                await SendText(text);

                await TouchAt(360, 340, 1);

                num++;
                total_action++;
            }
            await TouchAt(100, 0x4b, 5);
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

        public async Task SpamFriend(MessageToFriendConfig config)
        {
            bool finish = false;

            int countSuccess = 0;

            string[] profilesPage1 = null;
            string[] profilesPage2 = null;

            var fileCapture = await CaptureScreenNow();
            var friends = _zaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
            var points = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
            profilesPage1 = points.Select(x => x.Name).ToArray();

            while (!finish)
            {

                while (points.Count == 0)
                {
                    await ScrollFriendList(9);

                    fileCapture = await CaptureScreenNow();
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

                await Task.Delay(2000);

                var pointRowFriend = points.Pop();
                ProfileMessage profile = new ProfileMessage();
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && await ClickToChatFriendAt(profile, pointRowFriend.Point, config))
                {
                    //save
                    //Add Log
                    countSuccess++;
                }

                //finish = countSuccess == maxFriendToday;
            }
        }

        private async Task<bool> ClickToChatFriendAt(ProfileMessage profile, ScreenPoint point, MessageToFriendConfig config)
        {
            await TouchAt(point);
            await Task.Delay(2000);//wait to navigate chat screen

            //GrabInfomation
            await TouchAtIconTopRight();
            await Task.Delay(200);
            await TouchAt(Screen.ChatScreenProfileAvartar);
            await Task.Delay(200);

            var infoGrab = await GrabProfileInfo(profile.Name);
            CopyProfile(profile, infoGrab);

            await TouchAtIconTopLeft();//Back to chat screen
            await TouchAtIconTopLeft();//Close sidebar
            //End friend
            await TouchAt(Screen.ChatScreenTextField);
            await Task.Delay(300);
            await SendText(config.TextToFemale);
            await Task.Delay(500);
            //await TouchAt(Screen.ChatScreenSendButton);

            await Task.Delay(1000);

            await TouchAt(Screen.IconTopLeft);//Go Back

            //_dbContext.LogSpamFriend(friendName);

            return true;
        }

        public async Task SpamFriend2(MessageToFriendConfig config)
        {
            Thread.Sleep((int)(delaynet + delay));
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MainTabActivity");

            bool finish = false;
            while (!finish)
            {
                var fileCapture = await CaptureScreenNow();
                var points = _zaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                var stack = new Stack<FriendPositionMessage>(points.OrderByDescending(x => x.Point.Y));

                if (stack.Count() == 0)
                {

                }

                if (stack.Count() > 0)
                {
                    var pos = stack.Pop();

                    await TouchAt(pos.Point);
                    await Task.Delay(2000);//wait to navigate chat screen

                    await TouchAt(Screen.ChatScreenTextField);
                    await Task.Delay(300);
                    await SendText(config.TextToFemale);
                    await Task.Delay(500);
                    //await TouchAt(Screen.ChatScreenSendButton);

                    await Task.Delay(1000);

                    await TouchAt(Screen.IconTopLeft);//Go Back
                }
            }
        }

        public async Task ChatAllfriends(string text, string path)
        {

            Thread.Sleep((int)(delaynet + delay));
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MainTabActivity");

            await TouchAt(0x159, 0x4b, 2);
            Bitmap bitmap = new Bitmap(Screencapture("chatfr").FullName);
            new List<string>();
            int num = 0;
            for (int i = 600; i < 0x4b0; i++)
            {
                if (HexConverter(bitmap.GetPixel(0x2ac, i)) == "#158FC2")
                {
                    istop = true;
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
                        if (istop)
                        {
                            return;
                        }
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
            for (int i = 0; !istop && (i < numFriends); i++)
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
            while (!istop && (num < numfriends))
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

        public void enableKeyBoard()
        {
            InvokeProc("/c adb shell ime set com.android.adbkeyboard/.AdbIME");
            Thread.Sleep(500);
        }

        public bool getStop() => istop;

        private static string HexConverter(Color c) => ("#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2"));

        public async Task SendKey(KeyCode keycode, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    _device.ExecuteShellCommand("input keyevent " + (int)keycode, _receiver);
                    await Task.Delay(_settings.Delay.PressedKeyEvent);
                }
            }
            else
            {
                log.Error("Can't send keyevent");

            }
        }

        public async Task Login(string account, string password, string region)
        {
            enableKeyBoard();
            try
            {
                InvokeProc("/c adb shell am force-stop com.zing.zalo");

                await Task.Delay(_settings.Delay.WaitForceCloseApp);

                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.LoginUsingPWActivity");

                await Task.Delay(_settings.Delay.WaitLoginScreenOpened);

                //Open regions
                await TouchAt(90, 160, 1);
                Thread.Sleep((int)(delaynet + delay));
                await TouchAt(760, 0x4b, 2);
                await SendText(region);

                //Open username
                await TouchAt(90, 0xd4, 1);
                await TouchAt(650, 260, 1);
                for (int i = 0; i < 12; i++)
                {
                    await SendKey(KeyCode.AkeycodeDel);
                }
                await SendText(account);


                await TouchAt(640, 0x14f, 1);
                await SendText(password);
                await SendKey(KeyCode.AkeycodeEnter, 2);

                await Task.Delay(_settings.Delay.WaitLogin);
            }
            catch (Exception ex)
            {
                //Login(account, password, region);

                log.Error(ex);
            }
            await TouchAt(100, 0x4b, 1);
            await TouchAt(700, 610, 1);
            await TouchAt(100, 0x4b, 1);
        }

        public async Task Post(string text, string path)
        {
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MyInfoActivity");

            Thread.Sleep((int)(delay + delaynet));
            await TouchAt(0x2ff, 0x492, 1);
            Thread.Sleep(delay);
            if (path.Length != 0)
            {
                await TouchAt(110, 320, 1);
                await TouchAt(250, 600, 1);
                Thread.Sleep(delay);
                await TouchAt(200, 200, 1);
                //if ((mainForm.pack == "basic") || (mainForm.pack == "free"))
                //{
                //    Touch(230, 0x9b, 1);
                //    Touch(500, 0x9b, 1);
                //}
                //else 
                if (NoImg > 4)
                {
                    await TouchAt(230, 0x9b, 1);
                    await TouchAt(500, 0x9b, 1);
                    await TouchAt(0x2fd, 0x9b, 1);
                    await TouchAt(230, 420, 1);
                    await TouchAt(500, 420, 1);
                    await TouchAt(0x2fd, 420, 1);
                    await TouchAt(230, 0x2ad, 1);
                    await TouchAt(500, 0x2ad, 1);
                    await TouchAt(0x2fd, 0x2ad, 1);
                }
                else
                {
                    await TouchAt(230, 0x9b, 1);
                    await TouchAt(500, 0x9b, 1);
                    await TouchAt(0x2fd, 0x9b, 1);
                    await TouchAt(230, 420, 1);
                }
                await TouchAt(720, 0x492, 1);
                Thread.Sleep(delay);
                await TouchAt(700, 150, 1);
                await SendText(text);
                await TouchAt(0x2ff, 0x45, 1);
            }
            else
            {
                Thread.Sleep(delay);
                await TouchAt(700, 150, 1);
                await SendText(text);
                await TouchAt(0x2ff, 0x45, 1);
            }
        }

        public string ResultSpin(string str)
        {
            Random rnd = new Random();
            return Spintext(rnd, str);
        }

        public async Task<string> CaptureScreenNow()
        {
            var fileName = DateTime.Now.Ticks.ToString();

            InvokeProc($"/c adb shell screencap -p sdcard/DCIM/{fileName}.png");

            await Task.Delay(3000);

            InvokeProc($"/c adb pull sdcard/DCIM/{fileName}.png d:/{fileName}.png");

            await Task.Delay(3000);

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

        public async Task SendText(string text)
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

            await Task.Delay(_settings.Delay.PressedKeyEvent);
        }

        public async Task SetGPS(string lat, string longt)
        {
            InvokeProc("/c adb shell am force-stop com.cxdeberry.geotag");

            await Task.Delay(_settings.Delay.CloseMap);


            InvokeProc("/c adb shell am start -n com.cxdeberry.geotag/.MainActivity");

            await Task.Delay(_settings.Delay.OpenMap);

            await TouchAt(750, 50, 1);

            await SendText(lat + "," + longt);
            await SendKey(KeyCode.AkeycodeEnter);

            await TouchAt(750, 50, 1);

            await Task.Delay(_settings.Delay.CloseMap);

            await TouchAt(200, 0x438, 1);
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
        public async Task TouchAtIconTopLeft()
        {
            await TouchAt(_settings.Screen.IconTopLeft);
        }

        public async Task TouchAtIconTopRight()
        {
            await TouchAt(_settings.Screen.IconTopRight);
        }

        public async Task TouchAtIconBottomLeft()
        {
            await TouchAt(_settings.Screen.IconBottomLeft);
        }

        public async Task TouchAtIconBottomRight()
        {
            await TouchAt(_settings.Screen.IconBottomRight);
        }

        public async Task TouchAt(ScreenPoint point, int times = 1)
        {
            await TouchAt(point.X, point.Y, times);
        }

        public async Task TouchSwipe(ScreenPoint point1, ScreenPoint point2, int times = 1)
        {
            await TouchSwipe(point1.X, point1.Y, point2.X, point2.Y, times);
        }

        public async Task TouchAt(int x, int y, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    _device.ExecuteShellCommand($"input tap {x} {y}", _receiver);
                    await Task.Delay(_settings.Delay.TouchEvent);
                }
            }
            else
            {
                log.Error("Can't touch");
            }
        }

        public async Task TouchSwipe(int x1, int y1, int x2, int y2, int times = 1)
        {
            if ((_device != null) && (_device.State == DeviceState.Online))
            {
                for (int i = 0; i < times; i++)
                {
                    InvokeProc($"/c adb shell input swipe {x1} {y1} {x2} {y2}");
                    await Task.Delay(_settings.Delay.ScrollEvent);
                }
            }
            else
            {
                log.Error("Can't touch swipe");
            }
        }
        #endregion

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