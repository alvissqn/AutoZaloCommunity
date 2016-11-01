using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service
{
    public class ZaloAddFriendService : ZaloCommunityDistributeServiceBase, ISearchStrangerAction
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloAddFriendService));

        public ZaloAddFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing)
            : base(settings, dbContext, zaloImageProcessing)
        {
        }

        public void AddFriendNearBy(Filter filter)
        {
            var gender = filter.GenderSelection;
            var ageValues = filter.FilterAgeRange.Split("-".ToArray());
            var ageFrom = ageValues[0];
            var ageTo = ageValues[1];
            var numFriends = filter.NumberOfAction;

            GotoActivity(Activity.UserNearbyList);

            ConfigsSearchFriend(gender, ageFrom, ageTo);
            //I'm on Search Page
            TouchAt(Screen.IconTopRight);//Open Right SideBar
            Delay(500);
            TouchGenderOnSideBar(gender);

            var maxFriendToday = Settings.MaxFriendAddedPerDay - Settings.AddedFriendTodayCount;

            if (maxFriendToday > numFriends)
                maxFriendToday = numFriends;

            if (maxFriendToday < 0)
                maxFriendToday = 0;

            AddFriend(maxFriendToday, filter);
        }

        public void AddFriendByPhone(Filter filter)
        {
            var countSuccess = 0;
            var count = 0;

            string[] phonelist = filter.IncludePhoneNumbers.Split(";,|".ToArray());

            while (countSuccess < filter.NumberOfAction)
            {
                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");
                bool success = false;
                while (!success)
                {
                    Thread.Sleep(100);
                    SendText(phonelist[count++].ToString());
                    SendKey(KeyCode.AkeycodeEnter);
                    Thread.Sleep(4000);
                    //check is not available

                    if (ZaloImageProcessing.HasFindButton())
                    {
                        Console.WriteLine("!Lỗi, số đt không có");
                    }
                    else
                    {
                        var profile = GrabProfileInfo();
                        var addSuccess = AddFriendViaIconButton(profile, filter);
                        Console.WriteLine($"!Thêm bạn bằng số đt: {phonelist[count]} thành công.");
                        if (addSuccess)
                        {
                            countSuccess++;
                            success = true;
                        }
                    }
                }
            }
        }

        private void AddFriend(int maxFriendToday, Filter filter)
        {
            Console.WriteLine($"!bắt đầu thêm bạn. số bạn yêu cầu tối đa trong ngày hôm nay là {maxFriendToday}");
            var finish = false;

            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            Console.WriteLine("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotAdded((x) => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
            var points = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
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

                    if (!profilesPage2.Except(profilesPage1).Any())
                    {
                        Console.WriteLine("!hết bạn trong danh sách.");
                        return;
                    }

                    profilesPage1 = profilesPage2;
                }

                Delay(2000);

                var pointRowFriend = points.Pop();

                var profile = new ProfileMessage() { Name = pointRowFriend.Name };
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToAddFriendAt(profile, pointRowFriend.Point, filter))
                {
                    DbContext.AddProfile(profile);
                    countSuccess++;
                    Console.WriteLine($"!yêu cầu kết bạn [{countSuccess}]: {profile.Name} bị thành công.");
                }

                finish = countSuccess == maxFriendToday;
            }
        }

        private FriendPositionMessage[] GetPositionAccountNotAdded(Action<string[]> allPrrofiles)
        {
            var captureFiles = CaptureScreenNow();
            var names = ZaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => DbContext.ProfileSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t.ToArray();
        }

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
            ZaloHelper.CopyProfile(profile, info);

            if (!info.IsAddedToFriend)
            {
                return AddFriendViaIconButton(profile, filter);
            }
            else
            {
                Console.WriteLine($"!yêu cầu kết bạn: {profile.Name} bị từ hủy. Lý do: đã có tên trong cơ sở dữ liệu");
                TouchAtIconTopLeft(); //GoBack to friendList
                return false;
            }
        }

        private bool AddFriendViaIconButton(ProfileMessage profile, Filter filter)
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

            var textGreeting = profile.Gender == "Nam" ? filter.TextGreetingForMale : filter.TextGreetingForFemale;

            SendText(textGreeting);

            //Debug:
            TouchAtIconTopLeft();
            //TouchAt(Screen.AddFriendScreenOkButton); //TouchToAddFriend then goto profile

            Delay(300);

            TouchAtIconTopLeft(); //GoBack to friendList

            return true;

        }
        private bool ProcessIfShowDialogWaitRequestedFriendConfirm()
        {
            var file = CaptureScreenNow();
            var hasDialog = ZaloImageProcessing.IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(file, Screen);
            if (hasDialog)
            {
                TouchAt(Screen.AddFriendScreenWaitFriendConfirmDialog);

                return true;
            }

            return false;
        }

        public void ConfigsSearchFriend(GenderSelection gender, string ageFrom, string ageTo)
        {
            GotoActivity(Activity.UserNearbySettings);

            TouchGender(gender);
            TouchAgeRange(ageFrom, ageTo);
            Delay(300);
            TouchAt(Screen.ConfigSearchFriendUpdateButton);//TOUCH Update
        }

        private void TouchAgeRange(string ageFrom, string ageTo)
        {
            TouchAt(Screen.ConfigSearchFriendAgeCombobox);//touch to do tuoi
            Delay(300);

            TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//touch to age to
            SendText(ageFrom);

            TouchAt(Screen.ConfigSearchFriendAgeToTextField);//Touch to age from
            SendText(ageTo);

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
    }
}