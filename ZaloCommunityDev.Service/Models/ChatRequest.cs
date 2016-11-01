using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.Service.Models
{
    public enum ChatObjective
    {
        FriendInContactList=0,
        StrangerNearBy = 1,
        StrangerByPhone=2
    }

    public class ChatRequest
    {
        public ProfileMessage Profile { get; set; }

        public ChatObjective Objective { get; set; }

    }
}
