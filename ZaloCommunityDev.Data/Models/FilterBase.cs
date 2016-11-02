using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZaloCommunityDev.Shared;

namespace ZaloCommunityDev.Data.Models
{
    public abstract class FilterBase
    {
        [Key]
        public int Id { get; set; }

        public string AccountName { get; set; }

        public string ConfigName { get; set; }

        [Column("MaleGreeting")]
        public string TextGreetingForMale { get; set; }

        [Column("FemaleGreeting")]
        public string TextGreetingForFemale { get; set; }

        public string SentImageForMale { get; set; }

        public string SentImageForFemale { get; set; }

        public string FilterAgeRange { get; set; }

        public string IncludedPeopleNames { get; set; }

        public string ExcludePeopleNames { get; set; }

        public string IncludePhoneNumbers { get; set; }

        public string ExcludePhoneNumbers { get; set; }

        public int NumberOfAction { get; set; }

        [NotMapped]
        public GenderSelection GenderSelection { get; set; }

        public string GenderSelectionText
        {
            get
            {
                return GenderSelection.ToString();
            }
            set
            {
                GenderSelection = (GenderSelection)Enum.Parse(typeof(GenderSelection), value);
            }
        }

         public string Locations { get; set; }

        public string IgnoreRecentActionBefore { get; set; }
    }
}