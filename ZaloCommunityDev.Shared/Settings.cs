namespace ZaloCommunityDev.Shared
{
    public class Settings
    {

        public string AndroidDebugBridgeOsLocation { get; set; } = @"C:\Program Files\Leapdroid\VM";

        public ScreenInfo Screen { get; } = new ScreenInfo();

        public Delay Delay { get; set; } = new Delay();

        public int MaxFriendAddedPerDay { get; set; } = 30;

        public int AddedFriendTodayCount { get; set; } = 0;

    }
}
