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
            var filter = JsonConvert.DeserializeObject<Filter>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\filter.json"));

            //IKernel kernal = new StandardKernel();
            //kernal.Bind<IZaloImageProcessing>().To<ZaloImageProcessing>();
            //kernal.Bind<DatabaseContext>().ToSelf();
            //kernal.Bind<Settings>().ToConstant(settings);
            //kernal.Bind<ZaloAdbRequest>().ToSelf();

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
                    zaloAddFriendService.AddFriendNearBy(filter);

                    break;

                case RunnerConstants.addfriendbyphone:
                case "ket-ban-qua-dien-thoai":
                    zaloAddFriendService = new ZaloAddFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloAddFriendService.AddFriendByPhone(filter);

                    break;

                case RunnerConstants.sendmessagenearby:
                case "gui-tin-nhan-gan-day":
                    var zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageNearBy(filter);

                    break;

                case RunnerConstants.sendmessagebyphonenumber:
                case "gui-tin-nhan-qua-so-dien-thoai":
                    zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageByPhoneNumber(filter);

                    break;

                case RunnerConstants.sendmessagetofriendsincontacts:
                case "gui-tin-nhan-trong-danh-ba":
                    zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageToFriendInContactList(filter);

                    break;
            }

            throw new Exception("TASK COMPLETED");
        }
    }
}