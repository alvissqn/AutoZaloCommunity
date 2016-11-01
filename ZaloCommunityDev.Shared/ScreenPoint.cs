using System;

namespace ZaloCommunityDev.Shared
{
    public struct ScreenPoint
    {
        public ScreenPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public ScreenPoint Scale(double value) => new ScreenPoint(Convert.ToInt32(X * value), Convert.ToInt32(Y * value));

        public override string ToString() => $"{X}, {Y}";
    }
}