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

        public DbSet<AddFriendNearByConfigDto> AddFriendNearByConfigSet { get; set; }
        public DbSet<AddFriendByPhoneConfigDto> AddFriendByPhoneConfigSet { get; set; }
        public DbSet<MessageToFriendConfigDto> MessageToFriendConfigSet { get; set; }
        public DbSet<MessageToStrangerConfigDto> MessageToStrangerConfigSet { get; set; }

        public DbSet<LogActivityDto> LogActivitySet { get; set; }
        public DbSet<LogMessageSentToFriendDto> LogMessageSentToFriendSet { get; set; }
        public DbSet<LogMessageSentToStrangerDto> LogMessageSentToStrangerSet { get; set; }
        public DbSet<LogRequestAddFriendDto> LogRequestAddFriendSet { get; set; }
        public DbSet<ProfileDto> ProfileSet { get; set; }

        public AddFriendNearByConfig[] GetAddingFriendConfig()
            => AddFriendNearByConfigSet.ToArray().Select(Mapper.Map<AddFriendNearByConfigDto, AddFriendNearByConfig>).ToArray();

        public MessageToFriendConfig[] GetAutoSpamConfigs()
            => MessageToFriendConfigSet.ToArray().Select(Mapper.Map<MessageToFriendConfigDto, MessageToFriendConfig>).ToArray();

        private static string TodayText => DateTime.Now.Date.ToString("dd/MM/yyyy");

        public void AddProfile(ProfileMessage profile)
        {
            ProfileSet.Add(new ProfileDto { BirthdayText = profile.BirthdayText, Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female, Name = profile.Name });

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