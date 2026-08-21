using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// Used for the Group Chat Members
    /// </summary>

#nullable disable
    public class GroupChatMember
    {
        public int Id { get; set; }

        //ForeignKey for the Group Chat
        public int GroupChatId { get; set; }

        //Foreign Key for the UserId.
        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime JoinedAt { get; set; }

        public GroupChat GroupChat { get; set; }

        public User User { get; set; }

        public EngineRole EngineRole { get; set; }
    }
}
