using System;
using System.Linq;

namespace ZaloCommunityDev.Shared.Structures
{
    public class ProfileMessage
    {
        private string _birthdayText;
        public string Name { get; set; }
        public string Gender { get; set; }

        public Gender GenderValue()
        {
            if (Gender == null)
            {
                return Shared.Gender.Unknown;
            }

            return Gender.ToLower().Contains("na") ? Shared.Gender.Male : Shared.Gender.Female;
        }

        public string PhoneNumber { get; set; }
        public DateTime Birthday { get; set; }

        public string BirthdayText
        {
            get { return _birthdayText; }
            set
            {
                if (value.Any(c => char.IsDigit(c)))
                {
                    _birthdayText = value;
                }
                _birthdayText = string.Empty;
            }
        }

        public bool IsAddedToFriend { get; set; }
        public string Location { get; set; }

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