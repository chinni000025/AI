using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using Microsoft.AspNetCore.JsonPatch;
using static AIEngineConnectivity.Models.EngineProjectDtos;

namespace AIEngineConnectivity.Services
{
    public interface IConversationService
    {
        /// <summary>
        /// Needs to address all returns types object to required things.
        /// </summary>
        public Task<object> GetConversations(CancellationToken cancellationToken);
        public Task<object> GetFavouriteConversations(CancellationToken cancellationToken);
        public Task<object> GetConversationById(Guid conversationId, CancellationToken cancellationToken);
        public Task<object> SendMessage(Guid? conversationId, Guid? projectId, MessagePayload messagePayload, CancellationToken cancellationToken);
        public Task<object> DeleteConversation(Guid conversationId, CancellationToken cancellationToken);
        public Task<object> UpdateConversation(Guid conversationId, JsonPatchDocument<ConversationPathDTO> jsonPatchDocument, CancellationToken cancellationToken);
        public Task<Object> GetArchiveChatsAsync(ArchiveChatRequest archiveChatRequest, CancellationToken cancellationToken);
        public Task<Guid> CreateProjectAsync(CreateEmptyProject createEmptyProject, CancellationToken cancellation);
    }
}
