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
            => AutoAddFriendSessionConfigSet.ToArray().Select(Mapper.Map<AutoAddFriendSessionConfigDto, AddingFriendConfig>).ToArray();

        public AutoPostToFriendSessionConfig[] GetAutoSpamConfigs()
            => AutoPostToFriendSessionConfigSet.ToArray().Select(Mapper.Map<AutoPostToFriendSessionConfigDto, AutoPostToFriendSessionConfig>).ToArray();

        private static string TodayText => DateTime.Now.Date.ToString("dd/MM/yyyy");

        public void AddProfileAddFriend(ProfileMessage info)
        {
            FriendProfileInfoSet.Add(new FriendProfileInfoDto { BirthdayText = info.BirthdayText, Gender = info.Gender == "Nam" ? Gender.Male : Gender.Female, Name = info.Name });

            var item = LogActivitySet.FirstOrDefault(x => x.Date == TodayText);
            if (item == null)
            {
                LogActivitySet.Add(new LogActivityDto { Date = TodayText, AddedFriendCount = 1 });
            }
            else
            {
                item.AddedFriendCount++;
            }

            SaveChanges();
        }

        public int GetAddedFriendCount() => LogActivitySet.FirstOrDefault(x => x.Date == TodayText)?.AddedFriendCount ?? 0;
    }
}