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
    public class ZaloMessageToFriendService : ZaloCommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloMessageToFriendService));

        public ZaloMessageToFriendService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing)
            : base(settings, dbContext, zaloImageProcessing)
        {
        }

        public void SendMessageToFriend(Filter config)
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
                if (Screen.InfoRect.Contains(pointRowFriend.Point) && ClickToChatFriendAt(profile, pointRowFriend.Point, config))
                {
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
            ZaloHelper.CopyProfile(profile, infoGrab);

            TouchAtIconTopLeft();//Back to chat screen
            TouchAtIconTopLeft();//Close sidebar
            //End friend

            TouchAt(Screen.ChatScreenTextField);
            Delay(300);
            SendText(config.TextGreetingForFemale);
            Delay(500);

            if (!IsDebug)
            {
                TouchAt(Screen.ChatScreenSendButton);
            }

            Delay(1000);

            TouchAt(Screen.IconTopLeft);//Go Back

            //_dbContext.LogSpamFriend(friendName);

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