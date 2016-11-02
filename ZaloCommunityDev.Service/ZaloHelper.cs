using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service
{
    public static class ZaloHelper
    {
        public static void CopyProfile(ProfileMessage profile, ProfileMessage info)
        {
            profile.BirthdayText = info.BirthdayText;
            profile.Gender = info.Gender;
            profile.IsAddedToFriend = info.IsAddedToFriend;
            profile.Name = string.IsNullOrWhiteSpace(profile.Name) ? info.Name : profile.Name;
            profile.PhoneNumber = info.PhoneNumber;
        }

        public static string GetGreetingText(ProfileMessage profle, Filter filter)
        {
            if (profle.Gender == "Nam")
            {
                return filter.TextGreetingForMale;
            }
            else
            {
                return filter.TextGreetingForMale;
            }                
        }
    }
}