using Newtonsoft.Json;
using Ninject;
using System;
using System.IO;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Service.Models;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service
{
    class Program
    {
        const string WorkingFolderPath = @"WorkingSession";

        static void Main(string[] args)
        {
            //args = new[] { "send-message-near-by", "6ce8be4569f24e2ea6c04bb4aa3ae0fc" };
            var sessionId = args[1];

            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\setting.json"));
            var filter = JsonConvert.DeserializeObject<Filter>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\filter.json"));

            //IKernel kernal = new StandardKernel();

            //kernal.Bind<IZaloImageProcessing>().To<ZaloImageProcessing>();
            //kernal.Bind<DatabaseContext>().ToSelf();
            //kernal.Bind<Settings>().ToConstant(settings);
            //kernal.Bind<ZaloAdbRequest>().ToSelf();

            var ZaloImageProcessing = new ZaloImageProcessing();
            var DatabaseContext = new DatabaseContext();
            var Settings = settings;
            var ZaloAdbRequest = new ZaloAdbRequest(Settings);
            ZaloAdbRequest.StartAvd(settings.DeviceNumber);

            var ZaloLoginService = new ZaloLoginService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
            ZaloLoginService.Login(settings.User);

            Console.WriteLine($"Request:{args[0]} .SessionId:{sessionId}.");
            switch (args[0])
            {
                case "add-friend-near-by":
                case "ket-ban-gan-day":

                    var ZaloAddFriendService = new ZaloAddFriendService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
                    ZaloAddFriendService.AddFriendNearBy(filter);

                    break;

                case "add-friend-by-phone":
                case "ket-ban-qua-dien-thoai":
                    ZaloAddFriendService = new ZaloAddFriendService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
                    ZaloAddFriendService.AddFriendByPhone(filter);

                    break;

                case "send-message-near-by":
                case "gui-tin-nhan-gan-day":
                    var ZaloMessageToFriendService = new ZaloMessageToFriendService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
                    ZaloMessageToFriendService.SendMessageNearBy(filter);

                    break;

                case "send-message-by-phone-number":
                case "gui-tin-nhan-qua-so-dien-thoai":
                    ZaloMessageToFriendService = new ZaloMessageToFriendService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
                    ZaloMessageToFriendService.SendMessageByPhoneNumber(filter);

                    break;

                case "send-message-to-friends-in-contacts":
                case "gui-tin-nhan-trong-danh-ba":
                    ZaloMessageToFriendService = new ZaloMessageToFriendService(Settings, DatabaseContext, ZaloImageProcessing, ZaloAdbRequest);
                    ZaloMessageToFriendService.SendMessageToFriendInList(filter);

                    break;
            }
        }
    }
}
