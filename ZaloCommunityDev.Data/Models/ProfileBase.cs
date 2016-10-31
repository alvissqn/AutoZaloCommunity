using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    public abstract class ProfileBase
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string BirthdayText { get; set; }

        [NotMapped]
        public Gender Gender { get; set; }

        public string GenderText
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

        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}