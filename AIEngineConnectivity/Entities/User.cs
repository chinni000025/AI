namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// This Model is used for the Storing the User Details.
    /// </summary>

#nullable disable
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        public Boolean IsActive { get; set; }

        //Navigations

        // One user can have many Conversations 1 to many.
        // User --> Conversations
        public ICollection<Conversation> Conversations { get; set; }

        // Many to many relations many users --> many projects.
        // not direct relations via project members.
        // User <---- ProjectMembers ---> Projects.
        public ICollection<ProjectMember> ProjectMembers { get; set; }

        // Many to many relation many users --> many group
        // not direct relations via GroupChatMembers.
        // User <---- Group Chat Members ----> Group Chat
        public ICollection<GroupChatMember> GroupChatMembers { get; set; }

        // One to many for the Message Reactions.
        public ICollection<MessageReaction> MessageReactions { get; set; }

        // Many to many between the users -->
        // Conversation share with multi relationship with same table.
        // one user share and another user receives.
        // user <----> Conversation shares 
        //user <----> Conversation shares.

        // Conversations shared by.
        public ICollection<ConversationShare> ConversationsShareBy { get; set; }

        // Conversations shared with user.
        public ICollection<ConversationShare> ConversationsShareWith { get; set; }


        // one user can have many group chats.
        public ICollection<GroupChat> GroupChat { get; set; }

        // one user can have many group messages.
        public ICollection<GroupMessage> GroupMessage { get; set; }
    }
}
