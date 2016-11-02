using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("LogActivity")]
    public class LogActivityDto
    {
        [Key]
        public int Id { get; set; }

        [StringLength(10)]
        public string Date { get; set; } = DateTime.Now.Date.ToString("dd/MM/yyyy");

        public int AddedFriendCount { get; set; }

        public int PostFriendCount { get; set; }

        public int PostStrangerCount { get; set; }

        public string Account { get; set; }
    }
}
