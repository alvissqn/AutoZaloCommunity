using AutoMapper;
using System.Data.Entity;
using System.Linq;
using ZaloCommunityDev.DAL.Models;
using ZaloCommunityDev.Models;
using System;
using ZaloImageProcessing207.Structures;

namespace ZaloCommunityDev.DAL
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=ZaloCommunityDb")
        {


        }
        public DbSet<AutoAddFriendSessionConfigDto> AutoAddFriendSessionConfigSet { get; set; }
        public DbSet<FriendProfileInfoDto> FriendProfileInfoSet { get; set; }
        public DbSet<LogActivityDto> LogActivitySet { get; set; }
        public DbSet<AutoPostToStrangerSessionConfigDto> AutoPostToStrangerSessionConfigSet { get; set; }
        public DbSet<AutoPostToFriendSessionConfigDto> AutoPostToFriendSessionConfigSet { get; set; }

        public AddingFriendConfig[] GetAddingFriendConfig()
        {
            return AutoAddFriendSessionConfigSet.ToArray().Select(x=>Mapper.Map<AutoAddFriendSessionConfigDto, AddingFriendConfig>(x)).ToArray();
        }

        public AutoPostToFriendSessionConfig[] GetAutoSpamConfigs()
        {
            return AutoPostToFriendSessionConfigSet.ToArray().Select(x => Mapper.Map<AutoPostToFriendSessionConfigDto, AutoPostToFriendSessionConfig>(x)).ToArray();
        }
        string todayText => DateTime.Now.Date.ToString("dd/MM/yyyy");
        public void AddProfileAddFriend(ProfileMessage info)
        {
            FriendProfileInfoSet.Add(new FriendProfileInfoDto { BirthdayText = info.BirthdayText, Gender = info.Gender == "Nam" ? Gender.Male : Gender.Female, Name = info.Name });
           

            var item = LogActivitySet.FirstOrDefault(x => x.Date == todayText);
            if (item == null)
            {
                LogActivitySet.Add(new LogActivityDto { Date = todayText, AddedFriendCount = 1 });
            }
            else
            {
                item.AddedFriendCount++;
            }

            SaveChanges();
        }

        public int GetAddedFriendCount() => LogActivitySet.Where(x => x.Date == todayText).FirstOrDefault()?.AddedFriendCount ?? 0;
    }
}
