namespace ZaloCommunityDev.Models
{
    public class MessageToFriendConfig
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TextToFemale { get; set; }

        public string TextToMale { get; set; }

        public string OnlySpamPeopleNames { get; set; }

        public string ExceptSpamPeopleNames { get; set; }

        public string NumberOfSpamMail { get; set; }

        public override string ToString() => $"{Name}: Nữ{TextToFemale} Nam:{TextToMale}";
    }
}