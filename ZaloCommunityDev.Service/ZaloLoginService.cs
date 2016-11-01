using System;
using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Service.Models;

namespace ZaloCommunityDev.Service
{
    public class ZaloLoginService : ZaloCommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloLoginService));

        public ZaloLoginService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest ZaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, ZaloAdbRequest)
        {
        }

        public void Login(User user)
        {
            var account = user.Username;
            var password = user.Password;
            var region = user.Region;

            EnableAbdKeyoard();

            try
            {
                InvokeProc("/c adb shell am force-stop com.zing.zalo");

                Delay(Settings.Delay.WaitForceCloseApp);

                InvokeProc("/c adb shell am start -n com.zing.zalo/.ui.LoginUsingPWActivity");

                Delay(Settings.Delay.WaitLoginScreenOpened);

                //Open regions
                TouchAt(Screen.LoginScreenCountryCombobox);
                Delay(1000);
                TouchAt(Screen.IconTopRight);
                Delay(1100);
                SendText(region);
                Delay(1200);
                TouchAt(Screen.LoginScreenFirstCountryItem);
                Delay(500);

                //Enter username
                TouchAt(Screen.LoginScreenPhoneTextField);
                SendKey(KeyCode.AkeycodeMoveEnd);
                for (var i = 0; i < 12; i++)
                {
                    SendKey(KeyCode.AkeycodeDel);
                }
                SendText(account);

                //Enter Password
                Delay(100);
                TouchAt(Screen.LoginScreenPasswordTextField);
                SendText(password);
                TouchAt(Screen.LoginScreenOkButton);
                //SendKey(KeyCode.AkeycodeEnter, 2);

                Delay(Settings.Delay.WaitLogin);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}