using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    [Table("AutoPostToStrangerSessionConfig")]
    public class AutoPostToStrangerSessionConfigDto
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string TextToFemale { get; set; }

        public string TextToMale { get; set; }
    }

    [Table("AutoPostToFriendSessionConfig")]
    public class AutoPostToFriendSessionConfigDto
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string TextToFemale { get; set; }

        public string TextToMale { get; set; }

        public string OnlySpamPeopleNames { get; set; }

        public string ExceptSpamPeopleNames { get; set; }

        public string NumberOfSpamMail { get; set; }
    }
}
