namespace ZaloCommunityDev.Shared
{
    public class Settings
    {
        public string AndroidDebugBridgeOsLocation { get; set; } = @"C:\Program Files\Leapdroid\VM";

        public ScreenInfo Screen { get; } = new ScreenInfo();

        public Delay Delay { get; set; } = new Delay();

        public int MaxFriendAddedPerDay { get; set; } = 30;

        public int AddedFriendTodayCount { get; set; } = 0;
        public bool IsDebug { get; set; } = true;

        public User User { get; set; } = new User
        {
            Username = "0979864903",
            Password = "kimngan12345"
        };
    }
}