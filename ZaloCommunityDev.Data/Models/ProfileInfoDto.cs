using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("Profile")]
    public class ProfileDto : ProfileBase {
       public bool IsFriend { get; set; }
    }
}