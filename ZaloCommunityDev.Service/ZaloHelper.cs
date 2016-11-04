using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service
{
    public static class ZaloHelper
    {
        public static void CopyProfile(ref ProfileMessage profile, ProfileMessage info)
        {
            profile.BirthdayText = info.BirthdayText;
            profile.Gender = info.Gender;
            profile.IsAddedToFriend = info.IsAddedToFriend;
            profile.Name = string.IsNullOrWhiteSpace(profile.Name) ? info.Name : profile.Name;
            profile.PhoneNumber = info.PhoneNumber;
        }

        public static void Output(string text) => Console.WriteLine("ZALOSERVICE>> " + text);

        public static void SendCompletedTaskSignal()
        {
            Console.WriteLine("ZALOSERVICE>> @TASK COMPLETED");

            Process.GetCurrentProcess().Kill();
        }

        public static ZaloMessage[] GetZalomessages(ProfileMessage profile, Filter filter)
        {
            if (profile.GenderValue() == Gender.Male)
            {
                return ParseZaloMessage(filter.TextGreetingForMale);
            }
            else
            {
                return ParseZaloMessage(filter.TextGreetingForFemale);
            }
        }

        private static ZaloMessage[] ParseZaloMessage(string text)
        {
            var messages = new List<ZaloMessage>();
            var values = text.Split("\r\n".ToCharArray());
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var zaloMessage = new ZaloMessage
                {
                    Type = value.StartsWith("%") ? ZaloMessageType.Image : ZaloMessageType.Text,
                    Value = value
                };
                try
                {
                    if (zaloMessage.Type == ZaloMessageType.Image)
                    {
                        var f = new FileInfo(zaloMessage.Value.Substring(1));
                        zaloMessage.Value = f.FullName;
                    }
                }
                catch (Exception ex)
                {
                    Output("Không tìm thấy hình ảnh ở đường dẫn: " + zaloMessage.Value);
                    continue;
                }

                messages.Add(zaloMessage);
            }

            return messages.ToArray();
        }

        internal static void OutputLine()
        {
            Console.WriteLine("ZALOSERVICE>> --------------------------------");
        }
    }
}