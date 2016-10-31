using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

            Mapper.Initialize(cfg => {
                cfg.CreateMap<AutoAddFriendSessionConfigDto, AddingFriendConfig>();
                cfg.CreateMap< AutoPostToFriendSessionConfigDto, AutoPostToFriendSessionConfig>();
                cfg.CreateMap<ProfileMessage, ProfileMessage>();                
            });

            base.OnStartup(e);
        }
    }
}
