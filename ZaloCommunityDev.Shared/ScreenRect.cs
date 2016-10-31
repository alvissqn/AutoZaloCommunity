namespace ZaloCommunityDev.Shared
{
    public struct ScreenRect
    {

        public static ScreenRect FromPoints(int left, int top, int width, int height) => new ScreenRect(left, top, width, height);

        public static ScreenRect FromRect(int left, int top, int right, int bottom) => new ScreenRect(left, top, right, bottom, true);

        private ScreenRect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;

            Width = width;
            Height = height;

            Right = left + width;
            Bottom = top + height;

            Center = new ScreenPoint(left + Width / 2, top + Height / 2);
        }

        private ScreenRect(int left, int top, int right, int bottom, bool flag)
        {
            Left = left;
            Top = top;

            Right = right;
            Bottom = bottom;

            Width = right - left;
            Height = bottom - top;

            Center = new ScreenPoint(left + Width / 2, top + Height / 2);
        }

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public ScreenPoint Center { get; set; }

        public bool Contains(ScreenPoint point)=> point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
    }
}