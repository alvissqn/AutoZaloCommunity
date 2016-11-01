using Newtonsoft.Json;
using System;
using System.IO;
using ZaloCommunityDev.Data;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Service
{
    class Program
    {
        const string WorkingFolderPath = @"WorkingSession";

        static void Main(string[] args)
        {
            args = new string[] { "add-friend-near-by", "d71f70232b5949ae93754b1eb28e4b62", "0" };
            var sessionId = args[1];
            var deviceNameOrIndex = args[2];

            Console.WriteLine($"Request:{args[0]} .SessionId:{sessionId}. Device: {deviceNameOrIndex}");
            switch (args[0])
            {
                case "add-friend-near-by":
                    var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($@".\WorkingSession\{sessionId}\setting.json"));
                    var filter = JsonConvert.DeserializeObject<Filter>(File.ReadAllText($@".\WorkingSession\{sessionId}\filter.json"));

                    var imageProcessing = new ImageProcessing.ImageProcessing();
                    var service = new ZaloCommunityDistributeService(settings, new DatabaseContext(), imageProcessing);

                    service.StartAvd(deviceNameOrIndex);
                    service.AddFriendNearBy(filter);
                    break;
            }
        }
    }
}
