using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("Account")]
    public class UserDto
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public string Region { get; set; }
        public int Order { get; set; }
    }
}