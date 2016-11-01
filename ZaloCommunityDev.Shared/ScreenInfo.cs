namespace ZaloCommunityDev.Shared
{
    public class ScreenInfo
    {
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

        #endregion Profile Screen

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

        #endregion Advantage Search Friend

        #region Search Friend Near By

        public ScreenPoint SearchFriendSideBarFemaleOnlyTextItem { get; set; } = new ScreenPoint(964, 124);
        public ScreenPoint SearchFriendSideBarMaleOnlyTextItem { get; set; } = new ScreenPoint(964, 235);
        public ScreenPoint SearchFriendSideBarBothGenderTextItem { get; set; } = new ScreenPoint(964, 355);

        #endregion Search Friend Near By

        #region Add Friend Screen

        public ScreenPoint AddFriendScreenGreetingTextField { get; set; } = new ScreenPoint(1000,370);
        public ScreenPoint AddFriendScreenOkButton { get; set; } = new ScreenPoint(626, 530);

        public ScreenPoint AddFriendScreenWaitFriendConfirmDialog { get; set; } = new ScreenPoint(634, 1153);

        #endregion Add Friend Screen

        #region Friend Tab

        public int FriendTabCircleRadiusAvartarUserFrom { get; set; } = 56;
        public int FriendTabCircleRadiusAvartarUserTo { get; set; } = 68;

        #endregion Friend Tab

        #region Chat Screen
        public ScreenPoint ChatScreenTextField { get; set; } = new ScreenPoint(272, 1875);
        public ScreenPoint ChatScreenSendButton => IconBottomRight;
        public ScreenPoint ChatScreenProfileAvartar { get; set; } = new ScreenPoint(835, 160);
        #endregion Chat Screen

        #region Login Screen
        public ScreenPoint LoginScreenCountryCombobox { get; set; } = new ScreenPoint(250, 270);
        public ScreenPoint LoginScreenFirstCountryItem { get; set; } = new ScreenPoint(250, 270);
        public ScreenPoint LoginScreenPhoneTextField{ get; set; } = new ScreenPoint(250, 900);
        public ScreenPoint LoginScreenPasswordTextField { get; set; } = new ScreenPoint(250, 550);
        public ScreenPoint LoginScreenOkButton { get; set; } = new ScreenPoint(650, 760);
        public ScreenPoint HomeScreenFriendTab { get; set; } = new ScreenPoint(520, 110);
        #endregion
    }
}