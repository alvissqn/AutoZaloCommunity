using System;

namespace ZaloImageProcessing207.Structures
{
    public struct ProfileMessage
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Birthday { get;  set; }
        public string BirthdayText { get;  set; }
        public bool IsAddedToFriend { get; set; }

        public static bool IsEmpty(ProfileMessage profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.Name))
                return false;

            if (!string.IsNullOrWhiteSpace(profile.Gender))
                return false;

            if (!string.IsNullOrWhiteSpace(profile.PhoneNumber))
                return false;

            if (profile.Birthday != default(DateTime))
                return false;

            return true;
        }
    }
}

