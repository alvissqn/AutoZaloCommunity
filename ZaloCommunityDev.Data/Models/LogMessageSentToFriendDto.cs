using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("LogMessageSentToFriend")]
    public class LogMessageSentToFriendDto : MessageToProfile {}
}