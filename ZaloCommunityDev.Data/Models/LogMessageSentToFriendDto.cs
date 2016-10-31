using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    [Table("LogMessageSentToFriend")]
    public class LogMessageSentToFriendDto : MessageToProfile {}
}