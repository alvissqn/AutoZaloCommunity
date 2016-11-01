using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("MessageToStrangerNearByConfig")]
    public class MessageToStrangerNearByConfigDto : FilterBase { }

     [Table("MessageToStrangerByPhoneConfig")]
    public class MessageToStrangerByPhoneConfigDto : FilterBase { }
}