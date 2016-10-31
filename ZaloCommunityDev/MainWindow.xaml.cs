using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace ZaloCommunityDev
{
    public partial class MainWindow
    {
        //private readonly ZaloCommunityService _service;
        // private readonly IZaloImageProcessing _zaloImageProcessing;

        public MainWindow()
        {
            InitializeComponent();
            // _service = ServiceLocator.Current.GetInstance<ZaloCommunityService>();
            //  _zaloImageProcessing = ServiceLocator.Current.GetInstance<IZaloImageProcessing>();
        }

        private async void Okok(object sender, RoutedEventArgs e)
        {
            //await service.AddFriendNearBy("Nữ", "16", "23", 2);
            //  await _service.SetGPS("107.013530", "10.793295");
            //service.TouchSwipe(350, 500, 350, 405);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //  var capture = await _service.CaptureScreenNow();
            //   _zaloImageProcessing.GetProfile(capture, new ScreenInfo());
        }

    }
}