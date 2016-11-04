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
    public class ZaloAddFriendService : ZaloCommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloAddFriendService));

        public ZaloAddFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest zaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, zaloAdbRequest)
        {
        }

        public void AddFriendNearBy(Filter filter)
        {
            try
            {
                ZaloHelper.Output("Tiến hành kết bạn theo vị trí địa lý");

                var canAddedFriendToday = (Settings.MaxFriendAddedPerDay - DbContext.GetAddedFriendCount(Settings.User.Username));
                var numberOfAction = filter.NumberOfAction > canAddedFriendToday ? canAddedFriendToday : filter.NumberOfAction;
                if (numberOfAction <= 0)
                {
                    ZaloHelper.Output("Kết bạn tối đa trong ngày rồi");

                    return;
                }

                var gender = filter.GenderSelection;
                var ageValues = filter.FilterAgeRange.Split("-".ToArray());
                var ageFrom = ageValues[0];
                var ageTo = ageValues[1];

                GotoPage(Activity.UserNearbyList);

                ZaloHelper.Output("Thiết lập tùy chỉnh nâng cao");
                AddSettingSearchFriend(gender, ageFrom, ageTo);

                AddFriend(numberOfAction, filter);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
        }

        public void AddFriendByPhone(Filter filter)
        {
            try
            {
                var canAddedFriendToday = (Settings.MaxFriendAddedPerDay - DbContext.GetAddedFriendCount(Settings.User.Username));
                var numberOfAction = filter.NumberOfAction > canAddedFriendToday ? canAddedFriendToday : filter.NumberOfAction;
                if (numberOfAction == 0)
                {
                    ZaloHelper.Output("Kết bạn tối đa trong ngày rồi");

                    return;
                }

                var countSuccess = 0;
                var phonelist = new Stack<string>(filter.IncludePhoneNumbers.Split(";,|".ToArray()));
                while (countSuccess < numberOfAction)
                {
                    ZaloHelper.Output("Đang mở trang kết bạn qua số điện thoại");
                    GotoPage(Activity.FindFriendByPhoneNumber);

                    var success = false;
                    while (!success)
                    {
                        if (phonelist.Count == 0)
                        {
                            return;
                        }

                        var phoneNumber = phonelist.Pop();
                        if (DbContext.LogRequestAddFriendSet.FirstOrDefault(x => x.PhoneNumber == phoneNumber && x.Account == Settings.User.Username) != null)
                        {
                            ZaloHelper.Output($"Bỏ qua. Số điện thoại {phoneNumber} đã có tên trong danh sách");

                            continue;
                        }

                        Thread.Sleep(800);
                        ZaloHelper.Output($"Đang xóa ô nhập chữ");
                        DeleteWordInFocusedTextField();
                        ZaloHelper.Output($"Đang nhập số điện thoại :{phoneNumber}");

                        SendText(phoneNumber);
                        SendKey(KeyCode.AkeycodeEnter);
                        Thread.Sleep(4000);

                        //check is not available
                        ZaloHelper.Output("!Kiểm tra thông báo");

                        if (ZaloImageProcessing.HasFindButton(CaptureScreenNow(), Screen))
                        {
                            ZaloHelper.Output("!Lỗi, số đt không có");
                        }
                        else
                        {
                            var profile = GrabProfileInfo();

                            profile.PhoneNumber = phoneNumber;
                            string reason;
                            if (!filter.IsValidProfile(profile, out reason))
                            {
                                ZaloHelper.Output("Bỏ qua bạn này, lý do: "+ reason);
                                TouchAtIconTopLeft();
                                continue;
                            }
                            else
                            {
                                var addSuccess = AddFriendViaIconButton(profile, filter);
                                ZaloHelper.Output($"!Thêm bạn bằng số đt: {phoneNumber} thành công.");
                                if (addSuccess)
                                {
                                    countSuccess++;
                                    success = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
        }

        private void AddFriend(int numberOfAction, Filter filter)
        {
            ZaloHelper.Output($"!bắt đầu thêm bạn. số bạn yêu cầu tối đa trong ngày hôm nay là {numberOfAction}");

            var countSuccess = 0;
            string[] profilesPage1 = null;
            string[] profilesPage2 = null;
            ZaloHelper.Output("!đang tìm thông tin các bạn");
            var friendNotAdded = GetPositionAccountNotAdded(x => profilesPage1 = x).OrderByDescending(x => x.Point.Y);
            var stack = new Stack<FriendPositionMessage>(friendNotAdded);

            profilesPage1.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));
            ZaloHelper.Output($"!--------------------");
            friendNotAdded.ToList().ForEach(x => ZaloHelper.Output($"!các bạn chưa được gửi lời mời: {x.Name}"));
            while (countSuccess < numberOfAction)
            {
                while (stack.Count == 0)
                {
                    ZaloHelper.Output("!đang cuộn danh sách bạn");
                    ScrollList(9);

                    ZaloHelper.Output("!đang tìm thông tin các bạn");
                    friendNotAdded = (GetPositionAccountNotAdded(x => profilesPage2 = x)).OrderByDescending(x => x.Point.Y);
                    stack = new Stack<FriendPositionMessage>(friendNotAdded);

                    profilesPage1.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));
                    ZaloHelper.Output($"!--------------------");
                    friendNotAdded.ToList().ForEach(x => ZaloHelper.Output($"!các bạn chưa được gửi lời mời: {x.Name}"));

                    profilesPage2.ToList().ForEach(x => ZaloHelper.Output($"!tìm thấy bạn trên màn hình: {x}"));

                    if (!profilesPage2.Except(profilesPage1).Any())
                    {
                        ZaloHelper.Output("!hết bạn trong danh sách.");
                        return;
                    }
                    profilesPage1 = profilesPage2;
                }

                Delay(2000);

                var pointRowFriend = stack.Pop();

                var profile = new ProfileMessage
                {
                    Name = pointRowFriend.Name
                };

                if (Screen.InfoRect.Contains(pointRowFriend.Point) 
                    && ClickToAddFriendAt(profile, pointRowFriend.Point, filter))
                {
                    DbContext.AddProfile(profile, Settings.User.Username);
                    countSuccess++;
                    ZaloHelper.Output($"!yêu cầu kết bạn [{countSuccess}]: {profile.Name} bị thành công.");
                }
            }
        }

        private FriendPositionMessage[] GetPositionAccountNotAdded(Action<string[]> allPrrofiles)
        {
            var captureFiles = CaptureScreenNow();
            var names = ZaloImageProcessing.GetListFriendName(captureFiles, Screen);

            var t = names.Where(v => DbContext.LogRequestAddFriendSet.FirstOrDefault(x => x.Name == v.Name && x.Account == Settings.User.Username) == null).ToArray();

            allPrrofiles(names.Select(x => x.Name).ToArray());
            return t.ToArray();
        }

        // ReSharper disable once UnusedMember.Local
        private bool ClickToAddFriendAtRowPosition(ProfileMessage profile, int position, Filter filter)
            => ClickToAddFriendAt(profile, Screen.MenuPoint.Y * 2, (position - 1) * Screen.FriendRowHeight + Screen.FriendRowHeight / 2 + Screen.HeaderHeight, filter);

        private bool ClickToAddFriendAt(ProfileMessage profile, ScreenPoint point, Filter filter)
            => ClickToAddFriendAt(profile, point.X, point.Y, filter);

        private bool ClickToAddFriendAt(ProfileMessage profile, int x, int y, Filter filter)
        {
            ZaloHelper.Output($"!đã nhấn vào bạn: {profile.Name}");

            TouchAt(x, y);//TOUCH TO ROW_INDEX

            Delay(2000);
            //I''m on profile page

            var info = GrabProfileInfo(profile.Name);
            ZaloHelper.CopyProfile(ref profile, info);

            string reason;
            if (!filter.IsValidProfile(profile, out reason))
            {
                ZaloHelper.Output("Bỏ qua bạn này, lý do: " + reason);
                TouchAtIconTopLeft(); //GoBack to friendList
                return false;
            }


            if (!info.IsAddedToFriend)
            {
                return AddFriendViaIconButton(profile, filter);
            }
            else
            {
                ZaloHelper.Output($"!yêu cầu kết bạn: {profile.Name} bị từ hủy. Lý do: đã có tên trong cơ sở dữ liệu");
                TouchAtIconTopLeft(); //GoBack to friendList
                return false;
            }
        }

        private bool AddFriendViaIconButton(ProfileMessage profile, Filter filter)
        {
            ZaloHelper.Output($"!tiến hành gửi yêu cầu kết bạn: {profile.Name}");
            //Wait to navigate to profiles
            TouchAtIconBottomRight();//Touch to AddFriends
                                     //Wait to Navigate to new windows
            Delay(3000);

            var proccessedDialog = ProcessIfShowDialogWaitRequestedFriendConfirm();
            if (proccessedDialog)
            {
                ZaloHelper.Output($"!yêu cầu kết bạn: {profile.Name} bị từ chối. Lý do: đã gửi yêu cầu rồi");
                TouchAtIconTopLeft(); //GoBack to friendList
                return false;
            }

            TouchAt(Screen.AddFriendScreenGreetingTextField);

            var textGreeting = ZaloHelper.GetZalomessages(profile, filter)?.FirstOrDefault(x=>x.Type==ZaloMessageType.Text)?.Value;
            if (!string.IsNullOrWhiteSpace(textGreeting))
            {
                ZaloHelper.Output($"!gửi: {textGreeting}");
                SendText(textGreeting);

                if (Settings.IsDebug)
                {
                    TouchAtIconTopLeft();
                }
                else
                {
                    TouchAt(Screen.AddFriendScreenOkButton); //TouchToAddFriend, zalo auto goto profile after sent
                }
            }
            else
            {
               TouchAtIconTopLeft(); //GoBack to profile
            }

            Delay(300);

            TouchAtIconTopLeft(); //GoBack to friendList

            DbContext.LogAddFriend(profile, Settings.User.Username ,textGreeting);

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