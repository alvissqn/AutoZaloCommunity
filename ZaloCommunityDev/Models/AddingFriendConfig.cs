using ZaloCommunityDev.DAL.Models;

namespace ZaloCommunityDev.Models
{
    public class AddingFriendConfig
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AgeRange { get; set; }

        public GenderSelection GenderSelection { get; set; }

        public int WishAddedNumberFriendPerDay { get; set; }

        public string TextGreetingForMale { get; set; }

        public string TextGreetingForFemale { get; set; }

        public override string ToString() => $@"{Name}";
    }
}