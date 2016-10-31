
using System;

namespace ZaloCommunityDev.Models
{
    public struct ScreenPoint
    {
        public ScreenPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }

        public ScreenPoint Scale(double value)
        {
            X = Convert.ToInt32(X * value);
            Y = Convert.ToInt32(Y * value);

            return this;
        }
    }

    public struct ScreenRect
    {

        public static ScreenRect FromPoints(int left, int top, int width, int height)
        {
            return new ScreenRect(left, top, width, height);
        }

        public static ScreenRect FromRect(int left, int top, int right, int bottom)
        {
            return new ScreenRect(left, top, right, bottom, true);
        }

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

    public interface IScreenSize
    {

    }

    public class ScreenInfo
    {
        public ScreenInfo()
        {

        }

        public string Name { get; set; } = "1280x2048.dpi384";

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 2048;

        public int HeaderHeight { get; set; } = 115;
        public int FooterHeight { get; set; } = 115;
        public int MainMenuWidth { get; set; } = 78;

        public ScreenRect WorkingRect { get; set; } = ScreenRect.FromRect(0, 60, 1280, 1932);
        public ScreenRect InfoRect => ScreenRect.FromRect(0, WorkingRect.Top - HeaderHeight, 1280, WorkingRect.Bottom - FooterHeight);

        public ScreenPoint MenuPoint => new ScreenPoint(WorkingRect.Left + MainMenuWidth / 2, WorkingRect.Top + HeaderHeight / 2);

        public ScreenPoint IconBottomLeft => new ScreenPoint(WorkingRect.Left + FooterHeight / 2, WorkingRect.Bottom - FooterHeight / 2);
        public ScreenPoint IconBottomRight => new ScreenPoint(WorkingRect.Right - FooterHeight / 2, WorkingRect.Bottom - FooterHeight / 2);

        public ScreenPoint IconTopRight => new ScreenPoint(WorkingRect.Right - FooterHeight / 2, WorkingRect.Top + HeaderHeight / 2);
        public ScreenPoint IconTopLeft => new ScreenPoint(WorkingRect.Left + FooterHeight / 2, WorkingRect.Top + HeaderHeight / 2);

        public int ChatRowHeight { get; set; } = 188;
        public int FriendRowHeight { get; set; } = 153;
        public int ProfileInfoRowHeight { get; set; } = 120;

        #region Profile Screen

        public ScreenPoint ProfileScreenTabInfo { get; set; } = new ScreenPoint(1056, 864);
        #endregion


        #region Advantage Search Friend
        public ScreenPoint ConfigSearchFriendAgeCombobox { get; set; } = new ScreenPoint(610, 310).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendAgeFromTextField { get; set; } = new ScreenPoint(340, 620).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendAgeToTextField { get; set; } = new ScreenPoint(465, 620).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendAgeOkButton { get; set; } = new ScreenPoint(500, 790).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendGenderCombobox { get; set; } = new ScreenPoint(610, 180).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendMaleOnlyComboboxItem { get; set; } = new ScreenPoint(640, 315).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendFemaleOnlyComboboxItem { get; set; } = new ScreenPoint(640, 243).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendBothGenderComboboxItem { get; set; } = new ScreenPoint(640, 407).Scale(1.6d);
        public ScreenPoint ConfigSearchFriendUpdateButton { get; set; } = new ScreenPoint(361, 407).Scale(1.6d);
        #endregion

        #region Search Friend Near By

        public ScreenPoint SearchFriendSideBarFemaleOnlyTextItem { get; set; } = new ScreenPoint(964, 124);
        public ScreenPoint SearchFriendSideBarMaleOnlyTextItem { get; set; } = new ScreenPoint(964, 235);
        public ScreenPoint SearchFriendSideBarBothGenderTextItem { get; set; } = new ScreenPoint(964, 355);
        #endregion

        #region Add Friend Screen

        public ScreenPoint AddFriendScreenGreetingTextField { get; set; } = new ScreenPoint(102, 372);
        public ScreenPoint AddFriendScreenOkButton { get; set; } = new ScreenPoint(626, 524);

        public ScreenPoint AddFriendScreenWaitFriendConfirmDialog { get; set; } = new ScreenPoint(634, 1153);

        #endregion

        #region Friend Tab

        public int FriendTabCircleRadiusAvartarUserFrom { get; set; } = 56;
        public int FriendTabCircleRadiusAvartarUserTo { get; set; } = 68;

        #endregion

        #region Chat Screen

        public ScreenPoint ChatScreenTextField { get; set; } = new ScreenPoint(272, 1875);
        public ScreenPoint ChatScreenSendButton => IconBottomRight;
        public ScreenPoint ChatScreenProfileAvartar { get; set; } = new ScreenPoint(835,160);  

        #endregion

        public SpecificPoints Points = new SpecificPoints();

    }

    public class SpecificPoints
    {

    }
}
