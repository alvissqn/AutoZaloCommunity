namespace ZaloCommunityDev.Shared
{
    public class Delay
    {
        public int PressedKeyEvent { get; set; } = 200;
        public int TouchEvent { get; set; } = 200;
        public int ScrollEvent { get; set; } = 2000;
        public int BetweenActivity { get; set; } = 3000;
        public int WaitForceCloseApp { get; set; } = 1000;
        public int WaitLoginScreenOpened { get; set; } = 3000;
        public int WaitLogin { get; set; } = 4000;

        public int CloseMap { get; set; } = 1000;
        public int OpenMap { get; set; } = 5000;
    }
}