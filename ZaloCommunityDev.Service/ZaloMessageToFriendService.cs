using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;
using System;
using ZaloCommunityDev.Service.Models;

namespace ZaloCommunityDev.Service
{
    public class ZaloMessageToFriendService : ZaloCommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloMessageToFriendService));

        public ZaloMessageToFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing)
            : base(settings, dbContext, zaloImageProcessing)
        {
        }

        public void SendMessageToFriendInList(Filter filter)
        {
            var finish = false;

            var countSuccess = 0;

            string[] profilesPage1 = null;

            var fileCapture = CaptureScreenNow();
            var friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
            var points = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
            profilesPage1 = points.Select(x => x.Name).ToArray();

            while (!finish)
            {
                while (points.Count == 0)
                {
                    ScrollList(9);

                    fileCapture = CaptureScreenNow();
                    friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                    points = new Stack<FriendPositionMessage>(friends.OrderByDescending(x => x.Point.Y));
                    var profilesPage2 = points.Select(x => x.Name).ToArray();
                    if (!profilesPage2.Except(profilesPage1).Any())
                    {
                        return;
                    }

                    profilesPage1 = profilesPage2;
                }

                Delay(2000);

                var pointRowFriend = points.Pop();

                var request = new ChatRequest() { Profile = new ProfileMessage() { Name = pointRowFriend.Name }, Objective = ChatObjective.FriendInContactList };

                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToChatFriendAt(request, pointRowFriend.Point, filter))
                {
                    countSuccess++;
                }

                //finish = countSuccess == maxFriendToday;
            }
        }

        public void SendMessageByPhoneNumber(Filter filter)
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
                    var phoneNumber = phonelist[count++];
                    Thread.Sleep(100);
                    SendText(phoneNumber);
                    SendKey(KeyCode.AkeycodeEnter);
                    Thread.Sleep(4000);
                    //check is not available

                    if (ZaloImageProcessing.HasFindButton(CaptureScreenNow(), Screen))
                    {
                        Console.WriteLine("!Lỗi, số đt không có");
                    }
                    else
                    {
                        TouchAt(Screen.IconBottomLeft);
                        Delay(800);

                        var request = new ChatRequest { Profile = new ProfileMessage { PhoneNumber = phoneNumber }, Objective = ChatObjective.StrangerByPhone };
                        Chat(request, filter);
                    }
                }
            }
        }

        public void SendMessageNearBy(Filter filter)
        {
            var gender = filter.GenderSelection;
            var ageValues = filter.FilterAgeRange.Split("-".ToArray());
            var ageFrom = ageValues[0];
            var ageTo = ageValues[1];
            var numFriends = filter.NumberOfAction;

            GotoActivity(Activity.UserNearbyList);

            AddSettingSearchFriend(gender, ageFrom, ageTo);

            var maxFriendToday = Settings.MaxFriendAddedPerDay - Settings.AddedFriendTodayCount;

            if (maxFriendToday > numFriends)
                maxFriendToday = numFriends;

            if (maxFriendToday < 0)
                maxFriendToday = 0;

            ChatFriendNearBy(maxFriendToday, filter);
        }

        private FriendPositionMessage[] GetPositionAccountNotSent(Action<string[]> allPrrofiles)
        {
            var captureFiles = CaptureScreenNow();
            var names = ZaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => DbContext.LogMessageSentToFriendSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();
            var t2 = t.Where(v => DbContext.LogMessageSentToStrangerSet.FirstOrDefault(x => x.Name == v.Name) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t2.ToArray();
        }

        private void ChatFriendNearBy(int maxFriendToday, Filter filter)
        {
            Console.WriteLine($"!bắt đầu gửi tin cho bạn gần đây. Số bạn yêu cầu tối đa trong ngày hôm nay là {maxFriendToday}");
            var finish = false;

            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            Console.WriteLine("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotSent(x => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
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

                    friendNotAdded = (GetPositionAccountNotSent((x) => profilesPage2 = x)).OrderByDescending(x => x.Point.Y);
                    points = new Stack<FriendPositionMessage>(friendNotAdded);

                    profilesPage1.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
                    Console.WriteLine($"!--------------------");
                    friendNotAdded.ToList().ForEach((x) => Console.WriteLine($"!bạn chưa được gửi tin nhắn: {x}"));

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

                var request = new ChatRequest { Profile = new ProfileMessage() { Name = pointRowFriend.Name }, Objective = ChatObjective.StrangerNearBy };
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToChatFriendAt(request, pointRowFriend.Point, filter))
                {
                    DbContext.AddProfile(request.Profile);
                    countSuccess++;
                    Console.WriteLine($"!gửi tin nhắn tới: {request.Profile.Name} thành công. Số bạn đã gửi thành công trong phiên này là: {countSuccess}");
                }

                finish = countSuccess == maxFriendToday;
            }
        }

        private bool ClickToChatFriendAt(ChatRequest profile, ScreenPoint point, Filter filter)
        {
            TouchAt(point);
            Delay(2000);//wait to navigate chat screen
            return Chat(profile, filter);
        }

        private bool Chat(ChatRequest profile, Filter filter)
        {
            //GrabInfomation
            TouchAtIconTopRight();
            Delay(200);
            TouchAt(Screen.ChatScreenProfileAvartar);
            Delay(200);

            var infoGrab = GrabProfileInfo(profile.Profile.Name);
            ZaloHelper.CopyProfile(profile.Profile, infoGrab);

            TouchAtIconTopLeft();//Back to chat screen
            TouchAtIconTopLeft();//Close sidebar
            //End friend

            TouchAt(Screen.ChatScreenTextField);
            Delay(300);
            var textGreeting = filter.TextGreetingForFemale;
            SendText(textGreeting);
            Delay(500);

            if (!IsDebug)
            {
                TouchAt(Screen.ChatScreenSendButton);
            }

            Delay(1000);

            TouchAt(Screen.IconTopLeft);//Go Back

            switch (profile.Objective)
            {
                case ChatObjective.FriendInContactList:
                    DbContext.AddLogMessageSentToFriend(profile.Profile, textGreeting);

                    break;
                case ChatObjective.StrangerByPhone:
                case ChatObjective.StrangerNearBy:
                    DbContext.AddLogMessageSentToStranger(profile.Profile, textGreeting);

                    break;                    
            }

            return true;
        }
    }
}