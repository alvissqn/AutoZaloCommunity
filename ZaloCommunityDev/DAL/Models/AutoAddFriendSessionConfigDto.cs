using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    [Table("AddingFriendConfig")]
    public class AutoAddFriendSessionConfigDto
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string AgeRange { get; set; }

        [Column("MaleGreeting")]
        public string TextGreetingForMale { get; set; }

        [Column("FemaleGreeting")]
        public string TextGreetingForFemale { get; set; }

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

        public int WishAddedNumberFriendPerDay { get; set; }
    }
}