using AutoMapper;
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
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<AddFriendNearByConfigDto, AddFriendNearByConfig>();
                cfg.CreateMap<MessageToFriendConfigDto, MessageToFriendConfig>();
                cfg.CreateMap<ProfileMessage, ProfileMessage>();
            });

            base.OnStartup(e);
        }
    }
}