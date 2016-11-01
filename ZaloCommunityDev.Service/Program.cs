using Newtonsoft.Json;
using System;
using System.IO;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.ImageProcessing;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service
{
    class Program
    {
        const string WorkingFolderPath = @"WorkingSession";

        static void Main(string[] args)
        {
            //args = new [] { "add-friend-near-by", "d71f70232b5949ae93754b1eb28e4b62", "0" };
            var sessionId = args[1];
            var deviceNameOrIndex = args[2];

            Console.WriteLine($"Request:{args[0]} .SessionId:{sessionId}. Device: {deviceNameOrIndex}");
            switch (args[0])
            {
                case "add-friend-near-by":
                    var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\setting.json"));
                    var filter = JsonConvert.DeserializeObject<Filter>(File.ReadAllText($@".\{WorkingFolderPath}\{sessionId}\filter.json"));

                    var imageProcessing = new ZaloImageProcessing();
                    var dbContext = new DatabaseContext();

                    var loginService = new ZaloLoginService(settings, dbContext, imageProcessing);
                    loginService.Login(settings.User);

                    var service = new ZaloAddFriendService(settings, dbContext, imageProcessing);
                    service.StartAvd(deviceNameOrIndex);
                    service.AddFriendNearBy(filter);

                    break;
            }
        }
    }
}
