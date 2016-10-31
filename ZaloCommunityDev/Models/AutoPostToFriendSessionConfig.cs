namespace ZaloCommunityDev.Models
{
    public class AutoPostToFriendSessionConfig
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TextToFemale { get; set; }

        public string TextToMale { get; set; }

        public string OnlySpamPeopleNames { get; set; }

        public string ExceptSpamPeopleNames { get; set; }

        public string NumberOfSpamMail { get; set; }

        public override string ToString()
        {
            return $"{Name}: Nữ{TextToFemale} Nam:{TextToMale}";
        }
    }
}