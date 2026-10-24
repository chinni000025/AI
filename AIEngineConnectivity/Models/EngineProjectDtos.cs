namespace AIEngineConnectivity.Models
{
    public class EngineProjectDtos
    {
        public record CreateEmptyProject(string Name, string? Description);
        public record CurrentUserProject(Guid Id, string Name, string Description, bool IsArchieved, DateTime CreateAt, DateTime UpdatedAt);
        public record ProjectConversations(Guid ConversationId, string Title, DateTime UpdatedAt, DateTime CreatedAt);
    }
}
