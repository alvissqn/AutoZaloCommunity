using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.DAL;
using ZaloCommunityDev.Services;
using ZaloImageProcessing207;

namespace ZaloCommunityDev.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ZaloCommunityService>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<DatabaseContext>();

            SimpleIoc.Default.Register<IZaloImageProcessing, ImageProcessing>();
        }

        public ZaloCommunityService Service => ServiceLocator.Current.GetInstance<ZaloCommunityService>();
        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
    }
}