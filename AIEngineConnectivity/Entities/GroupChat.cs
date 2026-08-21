using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AIEngineConnectivity.Entities
{
#nullable disable

    /// <summary>
    /// This Model is used for the Group Chats
    /// </summary>
    public class GroupChat
    {
        // Primary id of the Table.
        public int Id { get; set; }

        // Foreign key to the users who created this group
        public int CreatedByUserId { get; set; }

        // Group Name.
        [MaxLength(100)]
        public string Name { get; set; }

        // Group Descriptions.
        [MaxLength(500)]
        public string Descriptions { get; set; }


        // Model used.
        [MaxLength(100)]
        public string ModelUsed { get; set; }

        //Created At.
        public DateTime CreatedAt { get; set; }

        // Last updated At.
        public DateTime UpdateAt { get; set; }


        // navigations.

        // One Group chat Many Group Messages are there

        public ICollection<GroupMessage> GroupMessages { get; set; }

        // One group chat many Group Chat Memebers.

        public ICollection<GroupChatMember> GroupChatMembers { get; set; }

        public User CreatedBy { get; set; }

    }
}
