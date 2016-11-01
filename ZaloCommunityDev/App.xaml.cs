using AutoMapper;
using GalaSoft.MvvmLight.Threading;
using System.Windows;
using ZaloCommunityDev.Data.Models;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

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
                cfg.CreateMap<MessageToStrangerNearByConfigDto, Filter>();
                cfg.CreateMap<MessageToStrangerByPhoneConfigDto, Filter>();

                cfg.CreateMap<Filter, AddFriendNearByConfigDto>();
                cfg.CreateMap<Filter, AddFriendByPhoneConfigDto>();
                cfg.CreateMap<Filter, MessageToFriendConfigDto>();
                cfg.CreateMap<Filter, MessageToStrangerNearByConfigDto>();
                cfg.CreateMap<Filter, MessageToStrangerByPhoneConfigDto>();

                cfg.CreateMap<ProfileMessage, ProfileMessage>();
            });

            base.OnStartup(e);
        }
    }
}