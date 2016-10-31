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

        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }

        public int Width { get; }
        public int Height { get; }

        public ScreenPoint Center { get; }

        public bool Contains(ScreenPoint point)=> point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
    }
}