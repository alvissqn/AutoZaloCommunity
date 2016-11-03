using System;
using System.Diagnostics;
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

        public static void Output(string text) => Console.WriteLine("ZALOSERVICE>> "+ text);
        public static void SendCompletedTaskSignal()
        {
            Console.WriteLine("ZALOSERVICE>> @TASK COMPLETED");

            Process.GetCurrentProcess().Kill();
        }

        public static string GetGreetingText(ProfileMessage profle, Filter filter)
        {
            if (profle.GenderValue() == Gender.Male)
            {
                return filter.TextGreetingForMale;
            }
            else
            {
                return filter.TextGreetingForFemale;
            }
        }

        internal static void OutputLine()
        {
            Console.WriteLine("ZALOSERVICE>> --------------------------------");
        }
    }
}