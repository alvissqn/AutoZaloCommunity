namespace ZaloCommunityDev.Shared
{
    public class Delay
    {
        public int PressedKeyEvent { get; set; } = 200;
        public int TouchEvent { get; set; } = 200;
        public int ScrollEvent { get; set; } = 2000;
        public int BetweenActivity { get; set; } = 3000;
        public int CloseMap { get; internal set; } = 1000;
        public int OpenMap { get; internal set; } = 5000;
        public int WaitForceCloseApp { get; internal set; } = 1000;
        public int WaitLoginScreenOpened { get; internal set; } = 3000;
        public int WaitLogin { get; internal set; } = 4000;
    }
}