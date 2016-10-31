using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloCommunityDev.DAL.Models
{
    [Table("LogMessageSentToStranger")]
    public class LogMessageSentToStrangerDto : MessageToProfile { }
}