using log4net;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Service.Models;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service
{
    public enum Activity
    {
        UserNearbySettings,
        UserNearbyList,
        FindFriendByPhoneNumber,
        LoginUsingPw,
        MainTab
    }

    public abstract class ZaloCommunityDistributeServiceBase : CommunityDistributeServiceBase
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(ZaloCommunityDistributeServiceBase));

        protected ZaloCommunityDistributeServiceBase(Settings settings, DatabaseContext dbContext, IZaloImageProcessing zaloImageProcessing, ZaloAdbRequest ZaloAdbRequest)
            : base(settings, dbContext, zaloImageProcessing, ZaloAdbRequest)
        {
        }

        protected void AddSettingSearchFriend(GenderSelection gender, string ageFrom, string ageTo)
        {
            GotoPage(Activity.UserNearbySettings);

            TouchGender(gender);
            TouchAgeRange(ageFrom, ageTo);
            Delay(300);
            TouchAt(Screen.ConfigSearchFriendUpdateButton);//TOUCH Update   to back to previous page

            TouchGenderOnSideBar(gender);
        }

        private void TouchAgeRange(string ageFrom, string ageTo)
        {
            TouchAt(Screen.ConfigSearchFriendAgeCombobox);//touch to do tuoi
            Delay(300);

            TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//touch to age to
            SendText(ageFrom);

            TouchAt(Screen.ConfigSearchFriendAgeToTextField);//Touch to age from
            SendText(ageTo);

            TouchAt(Screen.ConfigSearchFriendAgeFromTextField);//re-touch to age to

            Delay(200);

            TouchAt(Screen.ConfigSearchFriendAgeOkButton); //touch OK
            Delay(300);
        }

        private void TouchGender(GenderSelection gender)
        {
            TouchAt(Screen.ConfigSearchFriendGenderCombobox);
            Delay(200);
            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    TouchAt(Screen.ConfigSearchFriendMaleOnlyComboboxItem);
                    break;

                case GenderSelection.OnlyFemale:
                    TouchAt(Screen.ConfigSearchFriendFemaleOnlyComboboxItem);
                    break;

                default:
                    TouchAt(Screen.ConfigSearchFriendBothGenderComboboxItem);
                    break;
            }
        }

        private void TouchGenderOnSideBar(GenderSelection gender)
        {
            Delay(500);
            //I'm on Search Page
            TouchAt(Screen.IconTopRight);//Open Right SideBar
            Delay(500);

            switch (gender)
            {
                case GenderSelection.OnlyMale:
                    TouchAt(Screen.SearchFriendSideBarMaleOnlyTextItem);
                    break;

                case GenderSelection.OnlyFemale:
                    TouchAt(Screen.SearchFriendSideBarFemaleOnlyTextItem);
                    break;

                default:
                    TouchAt(Screen.SearchFriendSideBarBothGenderTextItem);
                    break;
            }
        }

        public void GotoPage(Activity activity)
        {
            const string activityStart = "/c adb shell am start -n";

            string arguments;

            switch (activity)
            {
                case Activity.UserNearbyList:
                    arguments = $@"{activityStart} com.zing.zalo/.ui.UserNearbyListActivity";

                    break;

                case Activity.UserNearbySettings:
                    arguments = $@"{activityStart} com.zing.zalo/.ui.UserNearbySettingsActivity";

                    break;

                case Activity.FindFriendByPhoneNumber:
                    arguments = $@"{activityStart} com.zing.zalo/.ui.FindFriendByPhoneNumberActivity";

                    break;

                case Activity.LoginUsingPw:
                    arguments = $@"{activityStart} com.zing.zalo/.ui.LoginUsingPWActivity";

                    break;

                case Activity.MainTab:
                    arguments = $@"{activityStart} com.zing.zalo/.ui.MainTabActivity";

                    break;

                default:
                    return;
            }

            Delay(Settings.Delay.BetweenActivity);

            InvokeProc(arguments);

            Delay(Settings.Delay.BetweenActivity);
        }
    }
}