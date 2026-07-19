namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
#nullable disable

    /// <summary>
    /// This Model is used to hold the Messages 
    /// </summary>
    public class Message
    {
        // Primary Id of the Messages.
        public long Id { get; set; }

        //Foreign Key for the Conversation (Not Null). Which conversation.
        public int ConversationId { get; set; }

        // Role like User or Assistant. Not Null
        public int RoleId { get; set; }

        // Messages between the user and Assistant. Not Null
        public string Content { get; set; }

        // Used for identify the ParentMessage Id ( Nullable).
        //For conversation branching edit & regenerate
        public long? ParentMessageId { get; set; }

        // When Message was Sent at. (not null).
        public DateTime MessageSentAt { get; set; }

        // Used to Identify message was deleted or not.
        public Boolean IsDeleted { get; set; } = false;


        ///<summary>
        /// Relation ships between message to Messages Reactions and Message Attachments.
        ///  Message --> Message Reactions (many to one).
        ///  Message --> Message Attachments(many to one).
        /// </summary>

        // Nullable -- > Used to define we can a parent message or not.
        public Message? ParentMessage { get; set; } // either we can have parent message or not


        // we can have n number child messages to the parent message.
        public ICollection<Message> ChildMessages { get; set; } = new List<Message>();

        // One Message have n number of Reactions
        public ICollection<MessageReaction> MessageReactions { get; set; }

        public ICollection<MessageAttachment> MessageAttachments { get; set; }

        public Conversation Conversation { get; set; }

        public EngineRole EngineRole { get; set; }

    }
}
