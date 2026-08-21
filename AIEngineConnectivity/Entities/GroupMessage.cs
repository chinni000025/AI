using AIEngineConnectivity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIEngineConnectivity.Entities
{
    public class GroupMessage
    {
        //Primary Id.
        public long Id { get; set; }

        //Foreign Key for the Group Chat.
        public int GroupChatId { get; set; }

        // Foreign Key for the User.
        public int SenderUserId { get; set; }

        public int RoleId { get; set; }

        public string Content { get; set; }

        public int TokensUsed { get; set; }

        public DateTime CreatedAt { get; set; }

        public Boolean IsDeleted { get; set; } = false;

        // navigations.

        // One to many relation ship with group message attachments.

        public ICollection<GroupMessageAttachment> Attachments { get; set; }

        public GroupChat GroupChat { get; set; }

        // Sender
        public User Sender { get; set; }

        public EngineRole EngineRole { get; set; }

    }
}
