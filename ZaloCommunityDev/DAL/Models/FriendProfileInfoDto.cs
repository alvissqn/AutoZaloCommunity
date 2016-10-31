using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }

    [Table("FriendProfileInfo")]
    public class FriendProfileInfoDto
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string BirthdayText { get; set; }

        [NotMapped]
        public Gender Gender { get; set; }

        public string GenderSelectionText
        {
            get
            {
                return Gender.ToString();
            }
            set
            {
                Gender = (Gender)Enum.Parse(typeof(Gender), value);
            }
        }

        public DateTime Created { get; set; } = DateTime.Now;
    }
}
