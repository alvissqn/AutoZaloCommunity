using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.Data.Models
{
    [Table("LogMessageSentToStranger")]
    public class LogMessageSentToStrangerDto : MessageToProfile { }
}