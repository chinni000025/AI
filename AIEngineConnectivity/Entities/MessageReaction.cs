using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
#nullable disable
    /// <summary>
    /// Used for Reactions to the Messages.
    /// </summary>
    public class MessageReaction
    {
        public int Id { get; set; }

        public long MessageId { get; set; }

        public int UserId { get; set; }

        public string ReactionType { get; set; }

        public DateTime CreateAt { get; set; }


        // Relationship to Messages Table

        public Message Messages { get; set; }

        public User User { get; set; }
    }
}
