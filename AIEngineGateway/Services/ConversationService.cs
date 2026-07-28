namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Helpers;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;

#nullable disable
    public class ConversationService : IConversationService
    {

        private readonly IUserService _UserService;
        private readonly IRepositoryWrapper _Repository;
        private readonly IAIOrchestrator _aIOrchestrator;
        public ConversationService(IUserService userService, IRepositoryWrapper repository,
            IAIOrchestrator aIOrchestrator)
        {
            _UserService = userService;
            _Repository = repository;
            _aIOrchestrator = aIOrchestrator;
        }

        public async Task<object> DeleteConversation(Guid conversationId, CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            var conversation = await _Repository.ConversationRepository
                    .GetConversation(conversationId, currentUser.UserId, cancellationToken);
            if (conversation is null)
                throw new Exception($"Conversation with Id {conversationId} Not Found");

            await _Repository.ConversationRepository.DeleteConversation(conversation);
            await _Repository.SaveChangesAsync(cancellationToken);

            return new
            {
                response = "Ok"
            };
        }

        public async Task<object> GetConversationById(Guid conversationId, CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            var conversation = await _Repository.ConversationRepository
                .GetConversationWithMessages(conversationId, currentUser.UserId, cancellationToken);
            return conversation;
        }

        public async Task<object> GetConversations(CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            var conversation = await _Repository.ConversationRepository
                                    .GetUserConversations(currentUser.UserId, cancellationToken);
            return conversation;
        }

        public async Task<object> GetFavouriteConversations(CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            var conversations = await _Repository.ConversationRepository.GetUserFavoriteConversations(currentUser.UserId, cancellationToken);
            return conversations;
        }

        public async Task<object> SendMessage(Guid? conversationId, MessagePayload messagePayload, CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            Conversation conversation;

            if (conversationId is null)
            {
                conversation = new Conversation
                {
                    ConversationId = Guid.NewGuid(),
                    UserId = int.Parse(currentUser.UserId),
                    Title = messagePayload.Content.Length > 50 ? messagePayload.Content[..30] + "..." : messagePayload.Content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ModelUsed = messagePayload.Model,
                };

                await _Repository.GetEngineRepo<Conversation>().AddAsync(conversation, cancellationToken);
                await _Repository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                conversation = await _Repository.ConversationRepository
                    .GetConversation(conversationId.Value, currentUser.UserId, cancellationToken);
                if (conversation is null)
                    throw new Exception("Conversation is empty");
            }

            var userMessage = new Message
            {
                ConversationId = conversation.Id,
                RoleId = 1,
                Content = messagePayload.Content,
                MessageSentAt = DateTime.UtcNow
            };

            await _Repository.GetEngineRepo<Message>().AddAsync(userMessage, cancellationToken);
            var history = await _Repository.ConversationRepository.LoadHistory(conversation.Id, cancellationToken);
            var aiRequest = new AIRequest
            {
                Prompt = messagePayload.Content,
                ConversationHistory = history,
                Model = messagePayload.Model,
                Provider = messagePayload.Provider
            };

            try
            {
                var response = await _aIOrchestrator.ChatAsync(aiRequest);
                var assistantMessage = new Message
                {
                    ConversationId = conversation.Id,
                    RoleId = 2,
                    Content = response.Output,
                    MessageSentAt = DateTime.UtcNow
                };

                await _Repository.GetEngineRepo<Message>().AddAsync(assistantMessage, cancellationToken);
                conversation.UpdatedAt = DateTime.UtcNow;
                conversation.LastMessageAt = DateTime.UtcNow;
                await _Repository.SaveChangesAsync(cancellationToken);

                return new
                {
                    conversationId = conversation.ConversationId,
                    title = conversation.Title,
                    isNewConversation = conversationId is null,
                    output = response.Output
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<object> UpdateConversation(Guid conversationId, [FromBody] JsonPatchDocument<ConversationPathDTO> jsonPatchDocument, CancellationToken cancellation)
        {
            var currentUser = _UserService.GetCurrentUser;
            var conversation = await _Repository.ConversationRepository
                .GetConversation(conversationId, currentUser.UserId, cancellation);
            if (conversation is null) throw new Exception("Conservations Not Found");

            ConversationPathDTO conversationPathDTO = new ConversationPathDTO
            {
                Title = conversation.Title,
                IsArchived = conversation.IsArchived,
                IsPinned = conversation.IsPinned,
                IsFavorite = conversation.IsFavorite,
                ModelUsed = conversation.ModelUsed
            };

            jsonPatchDocument.ApplyTo(conversationPathDTO);

            var operation = jsonPatchDocument.Operations.First();

            switch (operation.path)
            {
                case ConversationUpdatingPaths.Title:
                    conversation.Title = conversationPathDTO.Title;
                    break;
                case ConversationUpdatingPaths.IsPinned:
                    if (conversation.IsArchived && (conversationPathDTO.IsPinned ?? false))
                        throw new Exception("Archived Conversation Can't be Pinned");
                    conversation.IsPinned = conversationPathDTO.IsPinned ?? conversation.IsPinned;
                    break;
                case ConversationUpdatingPaths.IsArchived:
                    conversation.IsPinned = false;
                    conversation.IsFavorite = false;
                    conversation.IsArchived = conversationPathDTO.IsArchived ?? conversation.IsArchived;
                    break;
                case ConversationUpdatingPaths.IsFavorite:
                    if (conversation.IsArchived)
                        throw new Exception("Archived Conversation Can't be Favorite");
                    conversation.IsPinned = false;
                    conversation.IsFavorite = conversationPathDTO.IsFavorite ?? conversation.IsFavorite;
                    break;
                case ConversationUpdatingPaths.ModelUsed:
                    conversation.ModelUsed = conversationPathDTO.ModelUsed ?? conversation.ModelUsed;
                    break;
                default:
                    break;
            }
            await _Repository.SaveChangesAsync(cancellation);

            return new
            {
                response = "Update Conversation Successfully"
            };
        }

        public async Task<object> GetArchiveChatsAsync(ArchiveChatRequest archiveChatRequest, CancellationToken cancellationToken)
        {
            var userId = _UserService.GetCurrentUser;
            var result = await _Repository.ConversationRepository.GetUserArchiveConversation(userId.UserId, archiveChatRequest, cancellationToken);
            return result;
        }
    }
}
