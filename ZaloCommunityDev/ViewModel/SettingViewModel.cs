using GalaSoft.MvvmLight;
using System.Windows;
using ZaloCommunityDev.Models;

namespace ZaloCommunityDev.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        public string AndroidDebugBridgeOsLocation { get; set; } = @"C:\Program Files\Leapdroid\VM";

        public ScreenInfo Screen { get; } = new ScreenInfo();

        public Delay Delay { get; set; } = new Delay();

        public AddingFriendConfig AddingFriendConfig { get; set; }

        public string TextAddFriend { get; set; } = "Hi. ";
        public int MaxFriendAddedPerDay { get; set; } = 23;
        public int FriendAddedToDay { get; set; } = 0;
    }
}
