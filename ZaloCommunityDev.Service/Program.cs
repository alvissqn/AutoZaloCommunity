using System;
using Newtonsoft.Json;
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
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            //args = new[] { "send-message-near-by", "6ce8be4569f24e2ea6c04bb4aa3ae0fc" };
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
            zaloAdbRequest.StartAvd(settings.DeviceNumber);

            var zaloLoginService = new ZaloLoginService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
            zaloLoginService.Login(settings.User);

            switch (args[0])
            {
                case "add-friend-near-by":
                case "ket-ban-gan-day":

                    var zaloAddFriendService = new ZaloAddFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloAddFriendService.AddFriendNearBy(filter);

                    break;

                case "add-friend-by-phone":
                case "ket-ban-qua-dien-thoai":
                    zaloAddFriendService = new ZaloAddFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloAddFriendService.AddFriendByPhone(filter);

                    break;

                case "send-message-near-by":
                case "gui-tin-nhan-gan-day":
                    var zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageNearBy(filter);

                    break;

                case "send-message-by-phone-number":
                case "gui-tin-nhan-qua-so-dien-thoai":
                    zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageByPhoneNumber(filter);

                    break;

                case "send-message-to-friends-in-contacts":
                case "gui-tin-nhan-trong-danh-ba":
                    zaloMessageToFriendService = new ZaloMessageToFriendService(settings, databaseContext, zaloImageProcessing, zaloAdbRequest);
                    zaloMessageToFriendService.SendMessageToFriendInList(filter);

                    break;
            }
        }
    }
}
