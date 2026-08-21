using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// This model is used for the Conversation shares.
    /// </summary>

#nullable disable

    public class ConversationShare
    {
        // Primary key.
        public int Id { get; set; }

        // Foreign Key for the Conversation.
        public int ConversationId { get; set; }

        // Foreign Key for the User  // who shares
        public int ShareByUserId { get; set; }

        // Share with Accept null if it is link share // who receives.
        public int? ShareWithUserId { get; set; }

        // Permissions like Read  , write , Admin.
        public int PermissionId { get; set; }


        // Identify for link based sharing
        [MaxLength(100)]
        public string? ShareToken { get; set; }

        // Identify for Is it Link share or not.
        public Boolean IsLinkShare { get; set; } = false;

        public DateTime ShareAt { get; set; }

        // Expires At. -- > Auto Expires
        public DateTime? ExpiresAt { get; set; }


        // Relationships

        public Conversation Conversation { get; set; }

        // who shares
        public User ShareByUser { get; set; }

        //who receives.
        public User ShareWithUser { get; set; }

        public Permission Permission { get; set; }

    }

}
