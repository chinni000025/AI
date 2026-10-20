using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using static AIEngineConnectivity.Models.EngineProjectDtos;

namespace AIEngineConnectivity.Repositories
{
    public interface IConversationRepository
    {

        public Task<Conversation?> GetConversation(Guid conversationGuid, string userId, CancellationToken cancellationToken);

        public Task<List<ChatContext>> LoadHistory(int conversationId, CancellationToken cancellationToken);

        public Task AddConversation(Conversation conversation, CancellationToken cancellationToken);

        public Task<ConversationWithMessages?> GetConversationWithMessages(Guid conversationId, string userId,
            CancellationToken cancellationToken);

        public Task<List<ConversationDTO>> GetUserConversations(string userId, CancellationToken cancellationToken);

        public Task<List<ConversationDTO>> GetUserFavoriteConversations(string userId, CancellationToken cancellationToken);

        public Task DeleteConversation(Conversation conversation);
        public Task<PagedResponse<ArchiveChatItem>> GetUserArchiveConversation(String userId, ArchiveChatRequest archiveChatRequest,
            CancellationToken cancellationToken);
        public Task DeleteConversationAsync(Guid Id, CancellationToken cancellationToken);
        public Task<List<CurrentUserProject>> GetCurrentUserProjectAsync(int userId, bool isArchieved, CancellationToken cancellation);
    }
}