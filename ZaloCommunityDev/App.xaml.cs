using AutoMapper;
using GalaSoft.MvvmLight.Threading;
using System.Windows;
using ZaloCommunityDev.DAL.Models;
using ZaloCommunityDev.Models;
using ZaloImageProcessing207.Structures;

namespace ZaloCommunityDev
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherHelper.Initialize();
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<AddFriendNearByConfigDto, Filter>();
                cfg.CreateMap<AddFriendByPhoneConfigDto, Filter>();
                cfg.CreateMap<MessageToFriendConfigDto, Filter>();
                cfg.CreateMap<MessageToStrangerConfigDto, Filter>();

                cfg.CreateMap<ProfileMessage, ProfileMessage>();
            });

            base.OnStartup(e);
        }
    }
}