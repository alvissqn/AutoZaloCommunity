using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;
using System;

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
                var profile = new ProfileMessage();
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToChatFriendAt(profile, pointRowFriend.Point, filter))
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
                        TouchAt(Screen.IconBottomLeft);
                        Delay(800);
                        ProfileMessage profile = new ProfileMessage();
                        Chat(profile, filter);
                    }
                }
            }
        }

        public void SendMessageNearBy(Filter filter)
        {

        }

        private bool ClickToChatFriendAt(ProfileMessage profile, ScreenPoint point, Filter filter)
        {
            TouchAt(point);
            Delay(2000);//wait to navigate chat screen

            return Chat(profile, filter);
        }

        private bool Chat(ProfileMessage profile, Filter filter)
        {
            //GrabInfomation
            TouchAtIconTopRight();
            Delay(200);
            TouchAt(Screen.ChatScreenProfileAvartar);
            Delay(200);

            var infoGrab = GrabProfileInfo(profile.Name);
            ZaloHelper.CopyProfile(profile, infoGrab);

            TouchAtIconTopLeft();//Back to chat screen
            TouchAtIconTopLeft();//Close sidebar
            //End friend

            TouchAt(Screen.ChatScreenTextField);
            Delay(300);
            SendText(filter.TextGreetingForFemale);
            Delay(500);

            if (!IsDebug)
            {
                TouchAt(Screen.ChatScreenSendButton);
            }

            Delay(1000);

            TouchAt(Screen.IconTopLeft);//Go Back

            return true;
        }

        public void Post(string text, string path)
        {
            InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.MyInfoActivity");

            Thread.Sleep(100);
            TouchAt(0x2ff, 0x492);
            Thread.Sleep(100);
            if (path.Length != 0)
            {
                TouchAt(110, 320);
                TouchAt(250, 600);
                Thread.Sleep(100);
                TouchAt(200, 200);
                if (NoImg > 4)
                {
                    TouchAt(230, 0x9b);
                    TouchAt(500, 0x9b);
                    TouchAt(0x2fd, 0x9b);
                    TouchAt(230, 420);
                    TouchAt(500, 420);
                    TouchAt(0x2fd, 420);
                    TouchAt(230, 0x2ad);
                    TouchAt(500, 0x2ad);
                    TouchAt(0x2fd, 0x2ad);
                }
                else
                {
                    TouchAt(230, 155);
                    TouchAt(500, 155);
                    TouchAt(765, 155);
                    TouchAt(230, 420);
                }
                TouchAt(720, 0x492);
                Thread.Sleep(100);
                TouchAt(700, 150);
                SendText(text);
                TouchAt(0x2ff, 0x45);
            }
            else
            {
                Thread.Sleep(100);
                TouchAt(700, 150);
                SendText(text);
                TouchAt(0x2ff, 0x45);
            }
        }
    }
}