using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AIEngineConnectivity.Entities
{
#nullable disable

    /// <summary>
    /// This Model is Used for preserving the Conversation between the user and AI.
    /// </summary>
    public class Conversation
    {
        // Id of the Table.
        public int Id { get; set; }

        //GUID id for the conversations.
        public Guid ConversationId { get; set; } //used for the public.

        //Title of the Conversation ( Auto generated based on the First Message from the User
        // And Allows the user to rename it.)
        [MaxLength(200)]
        public string Title { get; set; }

        // Foreign Key to the user table ( NOT NULL).
        public int UserId { get; set; }

        // Foreign Key to the Project Table  ( Accepts Null because if it uncategorized ).
        public int? ProjectId { get; set; }

        //Model Used by the user.
        [MaxLength(100)]
        public string ModelUsed { get; set; }

        // Number Token consumed by the user. Not Null
        public int TokensUsed { get; set; } = 0;

        // Used for Identifying whether the message is pinned or not.
        public bool IsPinned { get; set; }

        //Used for Identifying Favorite.
        public bool IsFavorite { get; set; } = false;

        //Used for Identifying Achieved or not.
        public bool IsArchived { get; set; } = false;

        //Used for Identifying Deleted or not. --> Soft Delete.
        public bool IsDeleted { get; set; } = false;

        //Created At.
        public DateTime CreatedAt { get; set; }

        // Updated At.
        public DateTime UpdatedAt { get; set; }

        //Last Message.
        public DateTime? LastMessageAt { get; set; }


        //navigations.

        // Conversation may be belongs to Projects one to many with projects
        public Project? Project { get; set; }

        // one to many
        public ICollection<ConversationShare> ConversationShares { get; set; }

        // one to many
        public ICollection<Message> Messages { get; set; }

        public User User { get; set; }
    }
}
