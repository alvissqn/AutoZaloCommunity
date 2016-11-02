using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ZaloCommunityService>();
            SimpleIoc.Default.Register<DatabaseContext>();
            SimpleIoc.Default.Register<Settings>();
            SimpleIoc.Default.Register<ConsoleOutputViewModel>();

        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public ConsoleOutputViewModel ConsoleOutputViewModel => ServiceLocator.Current.GetInstance<ConsoleOutputViewModel>();
        public Settings Settings => ServiceLocator.Current.GetInstance<Settings>();
    }
}