namespace ZaloCommunityDev.Shared
{
    public class Settings
    {
        public string AndroidDebugBridgeOsWorkingLocation { get; set; }
        public string DeviceNumber { get; set; }

        public ScreenInfo Screen { get; } = new ScreenInfo();

        public Delay Delay { get; set; } = new Delay();

        public int MaxFriendAddedPerDay { get; set; }

        public int MaxMessageStrangerPerDay { get; set; }

        public bool IsDebug { get; set; } = true;

        public User User { get; set; }

        
    }
}