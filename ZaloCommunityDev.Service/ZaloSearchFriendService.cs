using System.Collections.Generic;
using System.Linq;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;
using System;
using ZaloCommunityDev.Service.Models;

namespace ZaloCommunityDev.Service
{
    public class ZaloSearchFriendService : ZaloCommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloSearchFriendService));

        public ZaloSearchFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest zaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, zaloAdbRequest)
        {
        }

        public void SearchFriendInContactList()
        {
            try
            {
                GotoPage(Activity.MainTab);

                Delay(1000);

                TouchAt(Screen.HomeScreenFriendTab);

                Delay(1000);
                ZaloHelper.Output("Đang phân tích dữ liệu");

                var fileCapture = CaptureScreenNow();
                var friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);

                ZaloHelper.OutputLine();
                friends.ToList().ForEach(x => ZaloHelper.Output(x.Name));
                ZaloHelper.OutputLine();

                var stack = new Stack<FriendPositionMessage>(friends.Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderByDescending(x => x.Point.Y));
                var profilesPage1 = stack.Select(x => x.Name).ToArray();
                while (true)
                {
                    while (stack.Count == 0)
                    {
                        ScrollList(9);

                        ZaloHelper.Output("Đang phân tích dữ liệu màn hình");
                        fileCapture = CaptureScreenNow();
                        friends = ZaloImageProcessing.GetFriendProfileList(fileCapture, Screen);

                        ZaloHelper.OutputLine();
                        friends.ToList().ForEach(x => ZaloHelper.Output(x.Name));
                        ZaloHelper.OutputLine();

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

                    var rowFriend = stack.Pop();

                    if (DbContext.ProfileSet.FirstOrDefault(x => x.Name == rowFriend.Name && x.Account == Settings.User.Username) != null)
                    {
                        ZaloHelper.Output($"Thu nhập thông tin bạn {rowFriend.Name} rồi");

                        continue;
                    }

                    if (!Screen.InfoRect.Contains(rowFriend.Point))
                        continue;

                    TouchAt(rowFriend.Point);
                    Delay(2000);

                    //GrabInfomation
                    TouchAtIconTopRight();
                    Delay(1000);
                    TouchAt(Screen.ChatScreenProfileAvartar);
                    Delay(2000);

                    var profile = new ProfileMessage() { Name = rowFriend.Name };

                    var infoGrab = GrabProfileInfo(profile.Name);

                    ZaloHelper.CopyProfile(ref profile, infoGrab);

                    TouchAtIconTopLeft();//Back to chat screen
                    Delay(400);
                    TouchAtIconTopLeft();//Close sidebar
                    Delay(400);
                    TouchAtIconTopLeft(); //Goback friend list
                    Delay(400);
                    DbContext.AddProfile(profile, Settings.User.Username, true);
                    Delay(400);
                }
            }
            catch (Exception ex) { _log.Error(ex); }
            finally
            {
                ZaloHelper.SendCompletedTaskSignal();
            }
        }

        public void SearchFriendsLocation(string areaNameOrCoordinator)
        {
            //I'm in page contact

        }
    }
}