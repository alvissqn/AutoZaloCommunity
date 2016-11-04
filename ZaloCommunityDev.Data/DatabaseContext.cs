using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.OleDb;
using System.Linq;
using AutoMapper;
using Dapper;
using log4net;
using ZaloCommunityDev.Data.Models;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Data
{
    public enum LogType
    {
        AddedFriend = 0,
        MessageFriend = 1,
        MessageToStranger = 2
    }

    public class DatabaseContext : DbContext
    {
        private readonly ILog _log = LogManager.GetLogger(nameof(DatabaseContext));

        public DatabaseContext() : base("name=ZaloCommunityDb")
        {
        }

        public DbSet<UserDto> UserSet { get; set; }
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

        private static string TodayText => DateTime.Now.Date.ToString("dd'/'MM'/'yyyy");

        public void AddProfile(ProfileMessage profile, string account, bool isFriend = false)
        {
            try
            {
                ProfileSet.Add(new ProfileDto
                {
                    BirthdayText = profile.BirthdayText,
                    Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                    Name = profile.Name,
                    PhoneNumber = profile.PhoneNumber,
                    Location = profile.Location,
                    Account = account,
                    IsFriend = isFriend
                });

                SaveChanges();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public void LogActivityCount(ProfileMessage profile, string account, LogType logtype)
        {
            var item = LogActivitySet.FirstOrDefault(x => x.Date == TodayText && account == x.Account);
            if (item == null)
            {
                LogActivitySet.Add(new LogActivityDto { Date = TodayText, Account = account });
            }

            SaveChanges();
            item = LogActivitySet.First(x => x.Date == TodayText && account == x.Account);

            switch (logtype)
            {
                case LogType.AddedFriend:
                    item.AddedFriendCount++;
                    break;

                case LogType.MessageFriend:
                    item.PostFriendCount++;
                    break;

                case LogType.MessageToStranger:
                    item.PostStrangerCount++;
                    break;
            }
            SaveChanges();
        }

        public int GetAddedFriendCount(string account) => LogActivitySet.FirstOrDefault(x => x.Date == TodayText && x.Account == account)?.AddedFriendCount ?? 0;

        public int GetMessageToStragerCount(string account) => LogActivitySet.FirstOrDefault(x => x.Date == TodayText && x.Account == account)?.PostStrangerCount ?? 0;

        public List<User> GetAccountList() => UserSet.ToArray().Select(Mapper.Map<UserDto, User>).ToList();

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
                        var dbItemConfig = AddFriendNearByConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
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
                        var dbItemConfig = MessageToFriendConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
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
                        var dbItemConfig = MessageToStrangerByPhoneConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
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
                        var dbItemConfig = MessageToStrangerNearByConfigSet.FirstOrDefault(x => x.Id == itemConfig.Id);
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

        public void LogAddFriend(ProfileMessage profile, string account, string textGreeting)
        {
            LogRequestAddFriendSet.Add(new LogRequestAddFriendDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                Location = profile.Location,
                PhoneNumber = profile.PhoneNumber,
                Account = account
            });
            SaveChanges();

            LogActivityCount(profile, account, LogType.AddedFriend);

            AddProfile(profile, account);
        }

        public void AddLogMessageSentToFriend(ProfileMessage profile, string account, string textGreeting)
        {
            LogMessageSentToFriendSet.Add(new LogMessageSentToFriendDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                Location = profile.Location,
                PhoneNumber = profile.PhoneNumber,
                Account = account
            });
            LogActivityCount(profile, account, LogType.MessageFriend);
        }

        public void AddLogMessageSentToStranger(ProfileMessage profile, string account, string textGreeting)
        {
            LogMessageSentToStrangerSet.Add(new LogMessageSentToStrangerDto
            {
                BirthdayText = profile.BirthdayText,
                Gender = profile.Gender == "Nam" ? Gender.Male : Gender.Female,
                MessageText = textGreeting,
                Name = profile.Name,
                Location = profile.Location,
                PhoneNumber = profile.PhoneNumber,
                Account = account
            });

            LogActivityCount(profile, account, LogType.MessageToStranger);
        }

        public LogActivityDto[] GetDailyActivity(DateTime? dateTime = null, string account = null)
        {
            LogActivityDto[] logs = null;
            if (dateTime == null)
            {
                if (account == null)
                {
                    logs = LogActivitySet
                        .OrderByDescending(x => x.Id)
                        .Take(30)
                        .ToArray();
                }
                else
                {
                    logs = LogActivitySet
                        .Where(x => x.Account == account)
                        .OrderByDescending(x => x.Id)
                        .Take(30)
                        .ToArray();
                }
            }
            else
            {
                var dateText = dateTime?.ToString("dd'/'MM'/'yyyy");
                if (account == null)
                {
                    logs = LogActivitySet
                        .Where(x => x.Date == dateText)
                       .OrderByDescending(x => x.Id)
                       .ToArray();
                }
                else
                {
                    logs = LogActivitySet
                       .Where(x => x.Date == dateText && x.Account == account)
                      .OrderByDescending(x => x.Id)
                      .ToArray();
                }
            }

            return logs.OrderByDescending(x => DateTime.Parse(x.Date)).ThenBy(x => x.Account).ToArray();
        }

        public ProfileDto[] GetAllProfile(string account)
        {
            using (var db = new OleDbConnection(ConfigurationManager.ConnectionStrings["ZaloCommunityDb"].ConnectionString))
            {
                var sqlString = $"SELECT * FROM {DataHelper.TableName<ProfileDto>()} WHERE ({DataHelper.GetPropertyName<ProfileDto, string>(x => x.Account)}=@Account)";
                var logs = (List<ProfileDto>)db.Query<ProfileDto>(sqlString, new { @Account = account });

                return logs.ToArray();
            }
        }

        public LogRequestAddFriendDto[] GetLogRequestAddFriends(DateTime dateTime, string account)
        {
            using (var db = new OleDbConnection(ConfigurationManager.ConnectionStrings["ZaloCommunityDb"].ConnectionString))
            {
                var sqlString = $"SELECT * FROM {DataHelper.TableName<LogRequestAddFriendDto>()} WHERE ({DataHelper.GetPropertyName<MessageToProfile, DateTime>(x => x.CreatedTime)} BETWEEN @from AND @to) and ({DataHelper.GetPropertyName<MessageToProfile, string>(x => x.Account)}=@Account)";
                var logs = (List<LogRequestAddFriendDto>)db.Query<LogRequestAddFriendDto>(sqlString, new { @from = dateTime.Date, @to = dateTime.Date.AddDays(1), @Account = account });

                return logs.ToArray();
            }
        }

        public LogMessageSentToFriendDto[] GetLogMessageSentToFriends(DateTime dateTime, string account)
        {
            using (var db = new OleDbConnection(ConfigurationManager.ConnectionStrings["ZaloCommunityDb"].ConnectionString))
            {
                var sqlString = $"SELECT * FROM {DataHelper.TableName<LogMessageSentToFriendDto>()} WHERE ({DataHelper.GetPropertyName<MessageToProfile, DateTime>(x => x.CreatedTime)} BETWEEN @from AND @to) and ({DataHelper.GetPropertyName<MessageToProfile, string>(x => x.Account)}=@Account)";
                var logs = (List<LogMessageSentToFriendDto>)db.Query<LogMessageSentToFriendDto>(sqlString, new { @from = dateTime.Date, @to = dateTime.Date.AddDays(1), @Account = account });

                return logs.ToArray();
            }
        }

        public LogMessageSentToStrangerDto[] GetLogMessageSentToStrangers(DateTime dateTime, string account)
        {
            using (var db = new OleDbConnection(ConfigurationManager.ConnectionStrings["ZaloCommunityDb"].ConnectionString))
            {
                var sqlString = $"SELECT * FROM {DataHelper.TableName<LogMessageSentToStrangerDto>()} WHERE ({DataHelper.GetPropertyName<MessageToProfile, DateTime>(x => x.CreatedTime)} BETWEEN @from AND @to) and ({DataHelper.GetPropertyName<MessageToProfile, string>(x => x.Account)}=@Account)";
                var logs = (List<LogMessageSentToStrangerDto>)db.Query<LogMessageSentToStrangerDto>(sqlString, new { @from = dateTime.Date, @to = dateTime.Date.AddDays(1), @Account = account });

                return logs.ToArray();
            }
        }
    }
}