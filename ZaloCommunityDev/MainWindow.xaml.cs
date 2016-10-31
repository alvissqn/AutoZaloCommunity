using Microsoft.Practices.ServiceLocation;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ZaloCommunityDev.DAL;
using ZaloCommunityDev.Services;
using ZaloCommunityDev.ViewModel;
using ZaloImageProcessing207;

namespace ZaloCommunityDev
{
    public partial class MainWindow : Window
    {
        private ZaloCommunityService service;
        private IZaloImageProcessing zaloImageProcessing;

        public MainWindow()
        {
            InitializeComponent();
            service = ServiceLocator.Current.GetInstance<ZaloCommunityService>();
            zaloImageProcessing = ServiceLocator.Current.GetInstance<IZaloImageProcessing>();
        }

        private async void OKOK(object sender, RoutedEventArgs e)
        {
            //await service.AddFriendNearBy("Nữ", "16", "23", 2);
            await service.SetGPS("107.013530", "10.793295");
            //service.TouchSwipe(350, 500, 350, 405);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var capture = await service.CaptureScreenNow();
            zaloImageProcessing.GetProfile(capture, new Models.ScreenInfo());
           
        }
    }
}
