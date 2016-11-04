using System;
using Newtonsoft.Json;
using System.IO;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Service.Models;
using ZaloCommunityDev.Shared;
using System.Linq;

namespace ZaloCommunityDev.Service
{
    internal class Program
    {
        private const string WorkingFolderPath = @"WorkingSession";

        private static void Main(string[] args)
        {
            if (args[1] == "last-filter")
            {
                var directory = Directory.GetDirectories($@".\{WorkingFolderPath}\").OrderByDescending(Directory.GetCreationTime).First();

                args[1] = directory.Split("\\".ToArray()).Last();
            }

             Console.OutputEncoding = System.Text.Encoding.UTF8;

            var sessionId = args[1];

            ZaloHelper.Output($"Request:{args[0]} .SessionId:{sessionId}.");

            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\setting.json"));
            Func<Filter> getFilter = () => JsonConvert.DeserializeObject<Filter>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\filter.json"));

            var zaloImageProcessing = new ZaloImageProcessing();
            var databaseContext = new DatabaseContext();
            var zaloAdbRequest = new ZaloAdbRequest(settings);
            if (!zaloAdbRequest.StartAvd(settings.DeviceNumber))
            {
                ZaloHelper.Output("Không tìm thấy thiết bị android nào.");
                return;
            }
#if RELEASE
            var zaloLoginService = new ZaloLoginService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
            zaloLoginService.Login(settings.User);
#endif
            switch (args[0])
            {
                case RunnerConstants.addfriendnearby:
                case "ket-ban-gan-day":

                    var zaloAddFriendService = new ZaloAddFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloAddFriendService.AddFriendNearBy(getFilter());

                    break;

                case RunnerConstants.addfriendbyphone:
                case "ket-ban-qua-dien-thoai":
                    zaloAddFriendService = new ZaloAddFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloAddFriendService.AddFriendByPhone(getFilter());

                    break;

                case RunnerConstants.sendmessagenearby:
                case "gui-tin-nhan-gan-day":
                    var zaloMessageToFriendService = new ZaloMessageService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageNearBy(getFilter());

                    break;

                case RunnerConstants.sendmessagebyphonenumber:
                case "gui-tin-nhan-qua-so-dien-thoai":
                    zaloMessageToFriendService = new ZaloMessageService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageByPhoneNumber(getFilter());

                    break;

                case RunnerConstants.sendmessagetofriendsincontacts:
                case "gui-tin-nhan-trong-danh-ba":
                    zaloMessageToFriendService = new ZaloMessageService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageToFriendInContactList(getFilter());

                    break;

                case RunnerConstants.searchallfriendsincontacts:
                case "tim-tat-ca-cac-ban-trong-danh-ba":
                    var  zaloSearchFriendService = new ZaloSearchFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloSearchFriendService.SearchFriendInContactList();

                    break;
            }
        }
    }
}