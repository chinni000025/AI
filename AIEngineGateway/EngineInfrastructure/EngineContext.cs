namespace AIEngineGateway.EngineInfrastructure
{
    using Microsoft.EntityFrameworkCore;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Entities;

    public abstract class EngineContext : DbContext
    {
        protected EngineContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<MessageReaction> MessageReactions { get; set; }

        public DbSet<MessageAttachment> MessageAttachments { get; set; }

        public DbSet<GroupChat> GroupChats { get; set; }

        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<GroupMessageAttachment> GroupMessageAttachments { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<ConversationShare> ConversationShares { get; set; }

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        public DbSet<EngineRole> EngineRoles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<PostMigration> PostMigrations { get; set; }

        public DbSet<EngineConnection> EngineConnections { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Step 1 --> 
            // one message --> number of child messages. (one to many). with same table.
            // Adding Message Table.
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ParentMessage)
                .WithMany(c => c.ChildMessages)
                .HasForeignKey(m => m.ParentMessageId)
                .OnDelete(DeleteBehavior.NoAction); // prevents for circular dependency errors.

            // messages --> Message Reactions. (one to many).
            modelBuilder.Entity<Message>()
                 .HasMany(m => m.MessageReactions)
                 .WithOne(r => r.Messages)
                 .HasForeignKey(r => r.MessageId)
                 .OnDelete(DeleteBehavior.Cascade);

            // Message --> Message Attachments. (one to many).
            modelBuilder.Entity<Message>()
                .HasMany(m => m.MessageAttachments)
                .WithOne(a => a.Messages)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Step 2 --> 
            // Group chat to group messages. (one to many).
            modelBuilder.Entity<GroupChat>()
                .HasMany(m => m.GroupMessages)
                .WithOne(c => c.GroupChat)
                .HasForeignKey(m => m.GroupChatId)
                .OnDelete(DeleteBehavior.Cascade);

            //group message --> Group message attachments. (one to many).
            modelBuilder.Entity<GroupMessage>()
                .HasMany(gm => gm.Attachments)
                .WithOne(gma => gma.GroupMessages)
                .HasForeignKey(gma => gma.GroupMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            //Step 3 --> 
            //Project--> Conversations (one to many).
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Conversations)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            //Step 4 -->
            //Conversation --> Conversation Shares (one to many)
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.ConversationShares)
                .WithOne(cs => cs.Conversation)
                .HasForeignKey(c => c.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            //Conversation --> Messages.
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            //Step 5 --> users --> Projects via Project Members
            // Many to Many via project Members table.
            // User <---- ProjectMembers ---> Projects.
            // one user --> project Members 1 to many
            modelBuilder.Entity<User>()
                .HasMany(u => u.ProjectMembers)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // project --> project Members 1 to many.
            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectMembers)
                .WithOne(pm => pm.Project)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            //Steps 6 --> Users --> Group Chats via Group Chat Members.
            // User <---- Group Chat Members ----> Group Chat
            //Many to many via Group Chat Members.

            //user --> Group Chat Member 1 to many.
            modelBuilder.Entity<User>()
                .HasMany(u => u.GroupChatMembers)
                .WithOne(gc => gc.User)
                .HasForeignKey(gc => gc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // group chats --> group chat members 1 to many.

            modelBuilder.Entity<GroupChat>()
                .HasMany(gc => gc.GroupChatMembers)
                .WithOne(gcm => gcm.GroupChat)
                .HasForeignKey(gcm => gcm.GroupChatId)
                .OnDelete(DeleteBehavior.NoAction); // don't do delete automatically if the parent is deleted.

            //Step 7 --> Users <---> Conversations shares.
            //Many to many with multi relationship with user
            // user ----> Conversation shares. // share by
            // User <----Conversations shares // share with
            modelBuilder.Entity<User>()
                .HasMany(u => u.ConversationsShareBy)
                .WithOne(cs => cs.ShareByUser)
                .HasForeignKey(cs => cs.ShareByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.ConversationsShareWith)
                .WithOne(cs => cs.ShareWithUser)
                .HasForeignKey(cs => cs.ShareWithUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // step 8 : user to conversation.
            // one user has many conversations. one to many.
            modelBuilder.Entity<User>()
                .HasMany(u => u.Conversations)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent circular error via conversation shares.
            //one user has many message reactions . one to many.
            modelBuilder.Entity<User>()
                .HasMany(u => u.MessageReactions)
                .WithOne(m => m.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Restrict); // already cascade via conversations --> Messages

            // step 9 : user to Group chats.
            // one user can create many group chats. one to many.
            // user--> group chats.
            modelBuilder.Entity<User>()
                .HasMany(u => u.GroupChat)
                .WithOne(gc => gc.CreatedBy)
                .HasForeignKey(gc => gc.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // won't delete if the user deletes


            // one user has many group messages are there.
            // user --> group messages one to many
            modelBuilder.Entity<User>()
                .HasMany(u => u.GroupMessage)
                .WithOne(gm => gm.Sender)
                .HasForeignKey(gm => gm.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            //step 10: Adding Roles and permissions.
            // role --> messages like user or assistant. one to many
            modelBuilder.Entity<EngineRole>()
                .HasMany(r => r.Messages)
                .WithOne(m => m.EngineRole)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            //role --> Group messages (one to many).
            modelBuilder.Entity<EngineRole>()
                .HasMany(r => r.GroupMessages)
                .WithOne(gm => gm.EngineRole)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            //role -->  project members (one to many).
            modelBuilder.Entity<EngineRole>()
                .HasMany(r => r.ProjectMembers)
                .WithOne(p => p.EngineRole)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            //role --> Group chat members. (one to many).
            modelBuilder.Entity<EngineRole>()
                .HasMany(r => r.GroupChatMembers)
                .WithOne(gcm => gcm.EngineRole)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            //Permissions.
            //permission --> Conversation shares (one to many)
            modelBuilder.Entity<Permission>()
                .HasMany(p => p.ConversationShares)
                .WithOne(c => c.Permission)
                .HasForeignKey(c => c.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);


            //Conversation Table public GUID
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => c.ConversationId)
                .IsUnique();

        }
    }
}
