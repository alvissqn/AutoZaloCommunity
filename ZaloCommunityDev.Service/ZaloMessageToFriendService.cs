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

        public ZaloMessageToFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest ZaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, ZaloAdbRequest)
        {
        }

        public void SendMessageToFriendInList(Filter filter)
        {
            var finish = false;

            var countSuccess = 0;

            string[] profilesPage1 = null;

            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MainTabActivity");
            Delay(1000);
            TouchAt(Screen.HomeScreenFriendTab);
            Delay(1000);
            Console.WriteLine($"Đang phân tích dữ liệu");

            var fileCapture = CaptureScreenNow();
            var friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
            var stack = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
            profilesPage1 = stack.Select(x => x.Name).ToArray();

            while (!finish)
            {
                while (stack.Count == 0)
                {
                    ScrollList(9);

                    fileCapture = CaptureScreenNow();
                    friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                    stack = new Stack<FriendPositionMessage>(friends.OrderByDescending(x => x.Point.Y));
                    var profilesPage2 = stack.Select(x => x.Name).ToArray();
                    if (!profilesPage2.Except(profilesPage1).Any())
                    {
                        return;
                    }

                    profilesPage1 = profilesPage2;
                }

                Delay(2000);

                var pointRowFriend = stack.Pop();

                if(DbContext.LogMessageSentToFriendSet.FirstOrDefault(x=>x.Name== pointRowFriend.Name) != null)
                {
                    Console.WriteLine($"Đã gửi tin cho bạn {pointRowFriend.Name} rồi");

                    continue;
                }

                var request = new ChatRequest() { Profile = new ProfileMessage() { Name = pointRowFriend.Name }, Objective = ChatObjective.FriendInContactList };

                if (Screen.InfoRect.Contains(pointRowFriend.Point))
                {
                    TouchAt(pointRowFriend.Point);
                    Delay(2000);//wait to navigate chat screen
                    NavigateToGrab(request);
                    if (Chat(request, filter))
                    {
                        countSuccess++;
                        DbContext.AddProfile(request.Profile);
                    }
                }
            }
        }

        public void SendMessageByPhoneNumber(Filter filter)
        {
            var canSentToday = (Settings.MaxMessageStrangerPerDay - DbContext.GetMessageToStragerCount());
            var numberOfAction = filter.NumberOfAction > canSentToday ? canSentToday : filter.NumberOfAction;
            if (numberOfAction <= 0)
            {
                Console.WriteLine("đã gửi hết số bạn trong ngày rồi");

                return;
            }

            string[] phonelist = filter.IncludePhoneNumbers.Split(";,|".ToArray());

            var countSuccess = 0;
            while (countSuccess < numberOfAction)
            {
                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.FindFriendByPhoneNumberActivity");
                bool success = false;
                var stack = new Stack<string>(phonelist);

                while (!success)
                {
                    if (stack.Count == 0)
                    {
                        return;
                    }
                    var phoneNumber = stack.Pop();

                    if (DbContext.LogMessageSentToStrangerSet.First(x => x.PhoneNumber == phoneNumber) != null)
                    {
                        Console.WriteLine($"Đã gửi tin cho số đt {phoneNumber} rồi");

                        continue;
                    }

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

                        var profile = GrabProfileInfo();
                        profile.PhoneNumber = phoneNumber;
                        var request = new ChatRequest { Profile = profile, Objective = ChatObjective.StrangerByPhone };

                        if(Chat(request, filter))
                        {
                            countSuccess++;
                            DbContext.AddProfile(request.Profile);
                        }
                    }
                }
            }
        }

        public void SendMessageNearBy(Filter filter)
        {
            var canSentToday = (Settings.MaxMessageStrangerPerDay - DbContext.GetMessageToStragerCount());
            var numberOfAction = filter.NumberOfAction > canSentToday ? canSentToday : filter.NumberOfAction;
            if (numberOfAction <= 0)
            {
                Console.WriteLine("đã gửi hết số bạn trong ngày rồi");

                return;
            }

            var gender = filter.GenderSelection;
            var ageValues = filter.FilterAgeRange.Split("-".ToArray());
            var ageFrom = ageValues[0];
            var ageTo = ageValues[1];
           
            GotoActivity(Activity.UserNearbyList);

            AddSettingSearchFriend(gender, ageFrom, ageTo);

            ChatFriendNearBy(numberOfAction, filter);
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
            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            Console.WriteLine("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotSent(x => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
            var points = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach((x) => Console.WriteLine($"!tìm thấy bạn trên màn hình: {x}"));
            Console.WriteLine($"!--------------------");
            friendNotAdded.ToList().ForEach((x) => Console.WriteLine($"!các bạn chưa được gửi lời mời: {x.Name}"));
            while (countSuccess < maxFriendToday)
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
                if (Screen.InfoRect.Contains(pointRowFriend.Point))
                {

                    TouchAt(pointRowFriend.Point);
                    Delay(2000);//wait to navigate chat screen
                    
                    var infoGrab = GrabProfileInfo(pointRowFriend.Name);
                    ZaloHelper.CopyProfile(request.Profile, infoGrab);

                    if (Chat(request, filter))
                    {

                        DbContext.AddProfile(request.Profile);
                        countSuccess++;
                    }

                    Console.WriteLine($"!gửi tin nhắn tới: {request.Profile.Name} thành công. Số bạn đã gửi thành công trong phiên này là: {countSuccess}");
                }
            }
        }

        public void NavigateToGrab(ChatRequest profile)
        {
            //GrabInfomation
            TouchAtIconTopRight();
            Delay(1000);
            TouchAt(Screen.ChatScreenProfileAvartar);
            Delay(2000);

            var infoGrab = GrabProfileInfo(profile.Profile.Name);
            ZaloHelper.CopyProfile(profile.Profile, infoGrab);

            TouchAtIconTopLeft();//Back to chat screen
            TouchAtIconTopLeft();//Close sidebar
            //End friend
        }

        private bool Chat(ChatRequest profile, Filter filter)
        {
            TouchAt(Screen.ChatScreenTextField);
            Delay(300);
            var chatText = ZaloHelper.GetGreetingText(profile.Profile, filter);
            SendText(chatText);
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
                    DbContext.AddLogMessageSentToFriend(profile.Profile, chatText);

                    break;
                case ChatObjective.StrangerByPhone:
                case ChatObjective.StrangerNearBy:
                    DbContext.AddLogMessageSentToStranger(profile.Profile, chatText);

                    break;                    
            }

            return true;
        }
    }
}