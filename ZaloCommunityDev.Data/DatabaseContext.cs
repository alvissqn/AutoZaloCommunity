using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using ZaloCommunityDev.Data.Models;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=ZaloCommunityDb")
        {
        }

        public DbSet<AddFriendNearByConfigDto> AddFriendNearByConfigSet { get; set; }
        public DbSet<AddFriendByPhoneConfigDto> AddFriendByPhoneConfigSet { get; set; }
        public DbSet<MessageToFriendConfigDto> MessageToFriendConfigSet { get; set; }
        public DbSet<MessageToStrangerNearByConfigDto> MessageToStrangerNearByConfigSet { get; set; }
        public DbSet<MessageToStrangerByPhoneConfigDto> MessageToStrangerByPhoneConfigSet { get; set; }

        public DbSet<LogActivityDto> LogActivitySet { get; set; }
        public DbSet<LogMessageSentToFriendDto> LogMessageSentToFriendSet { get; set; }
        public DbSet<LogMessageSentToStrangerDto> LogMessageSentToStrangerSet { get; set; }
        public DbSet<LogRequestAddFriendDto> LogRequestAddFriendSet { get; set; }
        public DbSet<ProfileDto> ProfileSet { get; set; }

        private static string TodayText => DateTime.Now.Date.ToString("dd/MM/yyyy");

        public void AddProfile(ProfileMessage profile)
        {
            ProfileSet.Add(new ProfileDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                Name = profile.Name,
                PhoneNumber = profile.PhoneNumber
            });

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

        public Filter[] GetFilter(string key)
        {
            switch (key)
            {
                case "AutoAddFriendByPhonePage":
                    return AddFriendByPhoneConfigSet.ToArray().Select(Mapper.Map<AddFriendByPhoneConfigDto, Filter>).ToArray();

                case "AutoAddFriendNearByPage":
                    return AddFriendNearByConfigSet.ToArray().Select(Mapper.Map<AddFriendNearByConfigDto, Filter>).ToArray();

                case "AutoSendMessageToFriendPage":
                    return MessageToFriendConfigSet.ToArray().Select(Mapper.Map<MessageToFriendConfigDto, Filter>).ToArray();

                case "AutoSendMessageToStrangerByPhonePage":
                    return MessageToStrangerByPhoneConfigSet.ToArray().Select(Mapper.Map<MessageToStrangerByPhoneConfigDto, Filter>).ToArray();

                case "AutoSendMessageToStrangerNearByPage":
                    return MessageToStrangerNearByConfigSet.ToArray().Select(Mapper.Map<MessageToStrangerNearByConfigDto, Filter>).ToArray();
            }

            return null;
        }

        public void SaveFilter(ObservableCollection<Filter> sources, string key)
        {
            switch (key)
            {
                case "AutoAddFriendByPhonePage":
                    foreach (var item in sources)
                    {
                        var itemConfig = Mapper.Map<Filter, AddFriendByPhoneConfigDto>(item);
                        var dbItemConfig = AddFriendByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
                        if (dbItemConfig == null)
                        {
                            itemConfig.Id = 0;
                            AddFriendByPhoneConfigSet.Add(itemConfig);
                        }
                        else
                        {
                            CopyFilter(item, dbItemConfig);
                        }
                    }
                    break;

                case "AutoAddFriendNearByPage":
                    foreach (var item in sources)
                    {
                        var itemConfig = Mapper.Map<Filter, AddFriendNearByConfigDto>(item);
                        var dbItemConfig = AddFriendByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
                        if (dbItemConfig == null)
                        {
                            itemConfig.Id = 0;
                            AddFriendNearByConfigSet.Add(itemConfig);
                        }
                        else
                        {
                            CopyFilter(item, dbItemConfig);
                        }
                    }
                    break;

                case "AutoSendMessageToFriendPage":
                    foreach (var item in sources)
                    {
                        var itemConfig = Mapper.Map<Filter, MessageToFriendConfigDto>(item);
                        var dbItemConfig = AddFriendByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
                        if (dbItemConfig == null)
                        {
                            itemConfig.Id = 0;
                            MessageToFriendConfigSet.Add(itemConfig);
                        }
                        else
                        {
                            CopyFilter(item, dbItemConfig);
                        }
                    }
                    break;

                case "AutoSendMessageToStrangerByPhonePage":
                    foreach (var item in sources)
                    {
                        var itemConfig = Mapper.Map<Filter, MessageToStrangerByPhoneConfigDto>(item);
                        var dbItemConfig = AddFriendByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
                        if (dbItemConfig == null)
                        {
                            itemConfig.Id = 0;
                            MessageToStrangerByPhoneConfigSet.Add(itemConfig);
                        }
                        else
                        {
                            CopyFilter(item, dbItemConfig);
                        }
                    }
                    break;

                case "AutoSendMessageToStrangerNearByPage":

                    foreach (var item in sources)
                    {
                        var itemConfig = Mapper.Map<Filter, MessageToStrangerNearByConfigDto>(item);
                        var dbItemConfig = AddFriendByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
                        if (dbItemConfig == null)
                        {
                            itemConfig.Id = 0;
                            MessageToStrangerNearByConfigSet.Add(itemConfig);
                        }
                        else
                        {
                            CopyFilter(item, dbItemConfig);
                        }
                    }
                    break;
            }
            SaveChanges();
        }

        private static void CopyFilter(Filter item, dynamic dbItemConfig)
        {
            dbItemConfig.ConfigName = item.ConfigName;
            dbItemConfig.SentImageForMale = item.SentImageForMale;
            dbItemConfig.SentImageForFemale = item.SentImageForFemale;
            dbItemConfig.IncludePhoneNumbers = item.IncludePhoneNumbers;
            dbItemConfig.ExcludePhoneNumbers = item.ExcludePhoneNumbers;
            dbItemConfig.ExcludePeopleNames = item.ExcludePeopleNames;
            dbItemConfig.FilterAgeRange = item.FilterAgeRange;
            dbItemConfig.GenderSelection = item.GenderSelection;
            dbItemConfig.IncludedPeopleNames = item.IncludedPeopleNames;
            dbItemConfig.Locations = item.Locations;
            dbItemConfig.NumberOfAction = item.NumberOfAction;
            dbItemConfig.TextGreetingForFemale = item.TextGreetingForFemale;
            dbItemConfig.TextGreetingForMale = item.TextGreetingForMale;
        }

        public void LogAddFriend(ProfileMessage profile, string textGreeting)
        {
            LogRequestAddFriendSet.Add(new LogRequestAddFriendDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                PhoneNumber = profile.PhoneNumber
            });
            SaveChanges();
        }

        public void AddLogMessageSentToFriend(ProfileMessage profile, string textGreeting)
        {
            LogMessageSentToFriendSet.Add(new LogMessageSentToFriendDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                PhoneNumber = profile.PhoneNumber
            });
        }

        public void AddLogMessageSentToStranger(ProfileMessage profile, string textGreeting)
        {
            LogMessageSentToStrangerSet.Add(new LogMessageSentToStrangerDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                PhoneNumber = profile.PhoneNumber
            });
        }
    }
}