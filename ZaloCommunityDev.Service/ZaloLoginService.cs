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

        public ZaloLoginService(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest zaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, zaloAdbRequest)
        {
        }

        public void Login(User user)
        {
            try
            {
                var count = 0;
                var finish = false;
                while (!finish && count < 5)
                {
                    EnableAbdKeyoard();
                    var account = user.Username;
                    var password = user.Password;
                    var region = user.Region;

                    ZaloHelper.Output("Đóng ZALO trước đó");
                    InvokeProc("/c adb shell am force-stop com.zing.zalo");
                    Delay(Settings.Delay.WaitForceCloseApp);

                    //InvokeProc("/c adb shell am start -n com.zing.zalo/.MainActivity");

                    ZaloHelper.Output("Tiến hành đăng nhập vào tài khoản: " + account);
                    GotoPage(Activity.LoginUsingPw);

                    Delay(Settings.Delay.WaitLoginScreenOpened);
                    if (!region.Equals("Vietnam"))
                    {
                        ZaloHelper.Output("Chọn khu vực trong cửa sổ mới" + region);
                        TouchAt(Screen.LoginScreenCountryCombobox);
                        Delay(1000);
                        TouchAt(Screen.IconTopRight);
                        Delay(1100);
                        SendText(region);
                        Delay(1200);
                        TouchAt(Screen.LoginScreenFirstCountryItem);
                        Delay(500);
                    }

                    ZaloHelper.Output("Đang gửi tên đăng nhập");
                    TouchAt(Screen.LoginScreenPhoneTextField);
                    Delay(200);
                    TouchAt(Screen.LoginScreenPhoneTextField);
                    Delay(200);

                    DeleteWordInFocusedTextField();

                    SendText(account);

                    ZaloHelper.Output("Đang gửi mật khẩu");
                    Delay(100);
                    TouchAt(Screen.LoginScreenPasswordTextField);
                    Delay(500);
                    SendText(password);
                    TouchAt(Screen.LoginScreenOkButton);
                    //SendKey(KeyCode.AkeycodeEnter, 2);

                    Delay(Settings.Delay.WaitLogin);
                    ZaloHelper.Output("Đang gửi thông tin đăng nhập");

                    if (ZaloImageProcessing.HasLoginButton(CaptureScreenNow(), Settings.Screen))
                    {
                        ZaloHelper.Output("Đăng nhập thất bại, đang thử lại");
                    }
                    else
                    {
                        finish = true;
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                ZaloHelper.Output("Có lỗi xảy ra");
            }
        }
    }
}