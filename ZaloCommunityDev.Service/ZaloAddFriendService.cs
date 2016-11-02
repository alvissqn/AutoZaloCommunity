using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;
using ZaloCommunityDev.Service.Models;

namespace ZaloCommunityDev.Service
{
    public class ZaloAddFriendService : ZaloCommunityDistributeServiceBase, ISearchStrangerAction
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloAddFriendService));

        public ZaloAddFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest ZaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, ZaloAdbRequest)
        {
        }

        public void AddFriendNearBy(Filter filter)
        {
            var canAddedFriendToday = (Settings.MaxFriendAddedPerDay - DbContext.GetAddedFriendCount());
            var numberOfAction = filter.NumberOfAction > canAddedFriendToday ? canAddedFriendToday : filter.NumberOfAction;
            if (numberOfAction <= 0)
            {
                Console.WriteLine("Kết bạn tối đa trong ngày rồi");

                return;
            }

            var gender = filter.GenderSelection;
            var ageValues = filter.FilterAgeRange.Split("-".ToArray());
            var ageFrom = ageValues[0];
            var ageTo = ageValues[1];

            GotoActivity(Activity.UserNearbyList);

            AddSettingSearchFriend(gender, ageFrom, ageTo);


            AddFriend(numberOfAction, filter);
        }

        public void AddFriendByPhone(Filter filter)
        {
            var canAddedFriendToday = (Settings.MaxFriendAddedPerDay - DbContext.GetAddedFriendCount());
            var numberOfAction = filter.NumberOfAction > canAddedFriendToday ? canAddedFriendToday : filter.NumberOfAction;
            if (numberOfAction == 0)
            {
                Console.WriteLine("Kết bạn tối đa trong ngày rồi");

                return;
            }

            var countSuccess = 0;
            var phonelist = new Stack<string>(filter.IncludePhoneNumbers.Split(";,|".ToArray()));           
            while (countSuccess < numberOfAction)
            {
                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");
                var success = false;
                while (!success)
                {
                    if (phonelist.Count == 0)
                    {
                        return;
                    }

                    var phoneNumber = phonelist.Pop();
                    if (DbContext.LogRequestAddFriendSet.FirstOrDefault(x => x.PhoneNumber == phoneNumber)!= null){
                        Console.WriteLine("Bạn này đã có tên trong danh sách");

                        continue;
                    }
                    Thread.Sleep(100);
                    for (var i = 0; i < 15; i++)
                    {
                        SendKey(KeyCode.AkeycodeDel);
                    }

                    SendText(phoneNumber);
                    SendKey(KeyCode.AkeycodeEnter);
                    Thread.Sleep(4000);
                    //check is not available
                    Console.WriteLine("!Kiểm tra thông báo");
                    if (ZaloImageProcessing.HasFindButton(CaptureScreenNow(), Screen))
                    {
                        Console.WriteLine("!Lỗi, số đt không có");
                    }
                    else
                    {
                        var profile = GrabProfileInfo();

                        profile.PhoneNumber = phoneNumber;
                        var addSuccess = AddFriendViaIconButton(profile, filter);
                        Console.WriteLine($"!Thêm bạn bằng số đt: {phoneNumber} thành công.");
                        if (addSuccess)
                        {
                            countSuccess++;
                            success = true;
                        }
                    }
                }
            }
        }

        private void AddFriend(int numberOfAction, Filter filter)
        {
            Console.WriteLine($"!bắt đầu thêm bạn. số bạn yêu cầu tối đa trong ngày hôm nay là {numberOfAction}");

            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            Console.WriteLine("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotAdded((x) => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
            var points = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
            Console.WriteLine($"!--------------------");
            friendNotAdded.ToList().ForEach((x) => Console.WriteLine($"!các bạn chưa được gửi lời mời: {x}"));
            while (countSuccess < numberOfAction)
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
            }
        }

        private FriendPositionMessage[] GetPositionAccountNotAdded(Action<string[]> allPrrofiles)
        {
            var captureFiles = CaptureScreenNow();
            var names = ZaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => DbContext.LogRequestAddFriendSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t.ToArray();
        }

        public bool ClickToAddFriendAtRowPosition(ProfileMessage profile, int position, Filter filter)
            => ClickToAddFriendAt(profile, Screen.MenuPoint.Y * 2, (position - 1) * Screen.FriendRowHeight + Screen.FriendRowHeight / 2 + Screen.HeaderHeight, filter);

        public bool ClickToAddFriendAt(ProfileMessage profile, ScreenPoint point, Filter filter)
            => ClickToAddFriendAt(profile, point.X, point.Y, filter);

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

            Console.WriteLine($"!gửi: {textGreeting}");
            SendText(textGreeting);

            if (Settings.IsDebug)
            {
                TouchAtIconTopLeft();
            }
            else
            {
                TouchAt(Screen.AddFriendScreenOkButton); //TouchToAddFriend then goto profile
            }

            Delay(300);

            TouchAtIconTopLeft(); //GoBack to friendList

            DbContext.LogAddFriend(profile, textGreeting);

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
    }
}