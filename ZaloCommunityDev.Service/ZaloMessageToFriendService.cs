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

        public ZaloMessageToFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest zaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, zaloAdbRequest)
        {
        }

        public void SendMessageToFriendInList(Filter filter)
        {
            try
            {
                var countSuccess = 0;

                GotoPage(Activity.MainTab);

                Delay(1000);
                TouchAt(Screen.HomeScreenFriendTab);
                Delay(1000);
                ZaloHelper.Output("Đang phân tích dữ liệu");

                var fileCapture = CaptureScreenNow();
                var friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                var stack = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
                var profilesPage1 = stack.Select(x => x.Name).ToArray();

                while (countSuccess <= filter.NumberOfAction)
                {
                    while (stack.Count == 0)
                    {
                        ScrollList(9);

                        ZaloHelper.Output("Đang phân tích dữ liệu màn hình");
                        fileCapture = CaptureScreenNow();
                        friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);
                        stack = new Stack<FriendPositionMessage>(friends.OrderByDescending(x => x.Point.Y));
                        var profilesPage2 = stack.Select(x => x.Name).ToArray();
                        if (!profilesPage2.Except(profilesPage1).Any())
                        {
                            ZaloHelper.Output("Hết danh sách");
                            return;
                        }

                        profilesPage1 = profilesPage2;
                    }

                    Delay(2000);

                    var pointRowFriend = stack.Pop();

                    if (DbContext.LogMessageSentToFriendSet.FirstOrDefault(x => x.Name == pointRowFriend.Name) != null)
                    {
                        ZaloHelper.Output($"Đã gửi tin cho bạn {pointRowFriend.Name} rồi");

                        continue;
                    }

                    var request = new ChatRequest
                    {
                        Profile = new ProfileMessage
                        {
                            Name = pointRowFriend.Name
                        },
                        Objective = ChatObjective.FriendInContactList
                    };

                    if (Screen.InfoRect.Contains(pointRowFriend.Point))
                    {
                        TouchAt(pointRowFriend.Point);
                        Delay(2000);

                        NavigateToProfileScreenFromChatScreenToGetInfoThenGoBack(request);

                        if (Chat(request, filter))
                        {
                            countSuccess++;
                            DbContext.AddProfile(request.Profile, Settings.User.Username);
                        }
                    }
                }
            }
            catch (Exception ex) { _log.Error(ex); }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
        }

        public void SendMessageByPhoneNumber(Filter filter)
        {
            try
            {
                var canSentToday = Settings.MaxMessageStrangerPerDay - DbContext.GetMessageToStragerCount( Settings.User.Username);
                var numberOfAction = filter.NumberOfAction > canSentToday ? canSentToday : filter.NumberOfAction;
                if (numberOfAction <= 0)
                {
                    ZaloHelper.Output("đã gửi hết số bạn trong ngày rồi");

                    return;
                }

                var phonelist = filter.IncludePhoneNumbers.Split(";,|".ToArray());

                var countSuccess = 0;
                while (countSuccess < numberOfAction)
                {
                    GotoPage(Activity.FindFriendByPhoneNumber);
                    var success = false;
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
                            ZaloHelper.Output($"Đã gửi tin cho số đt {phoneNumber} rồi");

                            continue;
                        }

                        Thread.Sleep(100);
                        SendText(phoneNumber);
                        SendKey(KeyCode.AkeycodeEnter);
                        Thread.Sleep(4000);

                        ZaloHelper.Output("!đang kiểm tra số điện thoại khả dụng");
                        if (ZaloImageProcessing.HasFindButton(CaptureScreenNow(), Screen))
                        {
                            ZaloHelper.Output("!Lỗi, số đt không có");
                        }
                        else
                        {
                            TouchAt(Screen.IconBottomLeft);
                            Delay(800);

                            var profile = GrabProfileInfo();
                            profile.PhoneNumber = phoneNumber;
                            var request = new ChatRequest { Profile = profile, Objective = ChatObjective.StrangerByPhone };

                            if (Chat(request, filter))
                            {
                                countSuccess++;
                                DbContext.AddProfile(request.Profile, Settings.User.Username);

                                success = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { _log.Error(ex); }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
        }

        public void SendMessageNearBy(Filter filter)
        {
            try
            {
                var canSentToday = Settings.MaxMessageStrangerPerDay - DbContext.GetMessageToStragerCount(Settings.User.Username);
                var numberOfAction = filter.NumberOfAction > canSentToday ? canSentToday : filter.NumberOfAction;
                if (numberOfAction <= 0)
                {
                    ZaloHelper.Output("đã gửi hết số bạn trong ngày rồi");

                    return;
                }

                var gender = filter.GenderSelection;
                var ageValues = filter.FilterAgeRange.Split("-".ToArray());
                var ageFrom = ageValues[0];
                var ageTo = ageValues[1];

                GotoPage(Activity.UserNearbyList);

                AddSettingSearchFriend(gender, ageFrom, ageTo);

                ChatFriendNearBy(numberOfAction, filter);
            }
            catch (Exception ex) { _log.Error(ex); }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
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
            ZaloHelper.Output($"!bắt đầu gửi tin cho bạn gần đây. Số bạn yêu cầu tối đa trong ngày hôm nay là {maxFriendToday}");
            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            ZaloHelper.Output("!đang tìm thông tin các bạn");
            var friendNotAdded = (GetPositionAccountNotSent(x => profilesPage1 = x)).OrderByDescending(x => x.Point.Y);
            var points = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));
            ZaloHelper.Output("!--------------------");
            friendNotAdded.ToList().ForEach(x => ZaloHelper.Output($"!các bạn chưa được gửi lời mời: {x.Name}"));
            while (countSuccess < maxFriendToday)
            {
                while (points.Count == 0)
                {
                    ZaloHelper.Output("!đang cuộn danh sách bạn");
                    ScrollList(9);

                    ZaloHelper.Output("!đang tìm thông tin các bạn");

                    friendNotAdded = GetPositionAccountNotSent(x => profilesPage2 = x).OrderByDescending(x => x.Point.Y);
                    points = new Stack<FriendPositionMessage>(friendNotAdded);

                    profilesPage1.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));
                    ZaloHelper.Output($"!--------------------");
                    friendNotAdded.ToList().ForEach(x => ZaloHelper.Output($"!bạn chưa được gửi tin nhắn: {x}"));

                    profilesPage2.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));

                    if (!profilesPage2.Except(profilesPage1).Any())
                    {
                        ZaloHelper.Output("!hết bạn trong danh sách.");

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
                        DbContext.AddProfile(request.Profile, Settings.User.Username);
                        countSuccess++;
                    }

                    ZaloHelper.Output($"!gửi tin nhắn tới: {request.Profile.Name} thành công. Số bạn đã gửi thành công trong phiên này là: {countSuccess}");
                }
            }
        }

        public void NavigateToProfileScreenFromChatScreenToGetInfoThenGoBack(ChatRequest profile)
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
                    DbContext.AddLogMessageSentToFriend(profile.Profile, Settings.User.Username, chatText);

                    break;

                case ChatObjective.StrangerByPhone:
                case ChatObjective.StrangerNearBy:
                    DbContext.AddLogMessageSentToStranger(profile.Profile, Settings.User.Username, chatText);

                    break;
            }

            return true;
        }
    }
}