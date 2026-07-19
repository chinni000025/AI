namespace AIEngineGateway.Repositories
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.EntityFrameworkCore;

    public class ConversationRepository : IConversationRepository
    {
        private readonly EngineContext _EngineContext;
        public ConversationRepository(EngineContext engineContext)
        {
            _EngineContext = engineContext;
        }

        public async Task AddConversationMessage(Message message, CancellationToken cancellationToken)
        {
            await _EngineContext.AddAsync(message, cancellationToken);
        }

        public async Task AddConversation(Conversation conversation, CancellationToken cancellationToken)
        {
            await _EngineContext.AddAsync(conversation, cancellationToken);
        }

        public async Task<Conversation?> GetConversation(Guid conversationGuid, string userId, CancellationToken cancellationToken)
        {
            return await _EngineContext.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationGuid
                && c.UserId.ToString() == userId, cancellationToken);
        }

        public async Task<List<ChatContext>> LoadHistory(int conversationId, CancellationToken cancellationToken)
        {
            var query = await (from m in _EngineContext.Messages
                               join r in _EngineContext.EngineRoles
                               on m.RoleId equals r.Id
                               where m.ConversationId == conversationId && !m.IsDeleted
                               orderby m.MessageSentAt descending
                               select new
                               {
                                   r.Name,
                                   m.Content,
                                   m.MessageSentAt
                               }).Take(5).ToListAsync(cancellationToken);

            var history = query.OrderBy(x => x.MessageSentAt)
                .Select(x => new ChatContext
                {
                    Role = x.Name,
                    Content = x.Content
                }).ToList();

            return history;
        }

        public async Task<ConversationWithMessages?> GetConversationWithMessages(Guid conversationId, string userId,
            CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var conversationWithMessages = await (from c in _EngineContext.Conversations
                                                  join m in _EngineContext.Messages
                                                  on c.Id equals m.ConversationId into messages
                                                  where c.ConversationId == conversationId && c.UserId == requiredUserId && !c.IsDeleted
                                                  select new ConversationWithMessages
                                                  {
                                                      ConversationId = c.ConversationId,
                                                      Title = c.Title,
                                                      ModelUsed = c.ModelUsed,
                                                      Messages = messages.Where(m => !m.IsDeleted)
                                                      .OrderBy(m => m.MessageSentAt).Select(m =>
                                                      new ConversationMessages
                                                      {
                                                          MessageId = m.Id,
                                                          RoleId = m.RoleId,
                                                          Content = m.Content,
                                                          MessagSentAt = m.MessageSentAt
                                                      }).ToList()
                                                  }).FirstOrDefaultAsync(cancellationToken);
            return conversationWithMessages;
        }

        public async Task<List<ConversationDTO>> GetUserConversations(string userId, CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var conversations = await (from c in _EngineContext.Conversations
                                       where c.UserId == requiredUserId && !c.IsDeleted && !c.IsFavorite && !c.IsArchived
                                       orderby c.LastMessageAt ?? c.CreatedAt descending
                                       select new ConversationDTO
                                       {
                                           ConversationId = c.ConversationId,
                                           ConversationTitle = c.Title,
                                           LastMessage = c.LastMessageAt,
                                           CreatedAt = c.CreatedAt,
                                           ModelUsed = c.ModelUsed,
                                           IsFavorite = c.IsFavorite,
                                           IsPinned = c.IsPinned
                                       }).ToListAsync(cancellationToken);
            return conversations;
        }

        public async Task<List<ConversationDTO>> GetUserFavoriteConversations(string userId, CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var favoriteConversations = await (from con in _EngineContext.Conversations
                                               where con.UserId == requiredUserId && con.IsFavorite && !con.IsDeleted
                                               orderby con.LastMessageAt ?? con.CreatedAt
                                               select new ConversationDTO
                                               {
                                                   ConversationId = con.ConversationId,
                                                   ConversationTitle = con.Title,
                                                   LastMessage = con.LastMessageAt,
                                                   CreatedAt = con.CreatedAt,
                                                   ModelUsed = con.ModelUsed,
                                                   IsFavorite = con.IsFavorite,
                                                   IsPinned = con.IsPinned
                                               }).ToListAsync(cancellationToken);
            return favoriteConversations;
        }

        public async Task DeleteConversation(Conversation conversation)
        {
            conversation.IsDeleted = true;
            conversation.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<PagedResponse<ArchiveChatItem>> GetUserArchiveConversation(string userId, ArchiveChatRequest archiveChatRequest,
            CancellationToken cancellationToken)
        {

            int requiredUserId = int.Parse(userId);
            var query = from arch in _EngineContext.Conversations
                        where arch.UserId == requiredUserId && !arch.IsDeleted && arch.IsArchived
                        select arch;

            if (!string.IsNullOrWhiteSpace(archiveChatRequest.Search))
                query = query.Where(arch => arch.Title.Contains(archiveChatRequest.Search));

            int totalCount = await query.CountAsync(cancellationToken);
            int skipCount = (archiveChatRequest.Page - 1) * archiveChatRequest.PageSize;

            var paginatedData = await query.OrderByDescending(arc => arc.UpdatedAt)
                                        .Skip(skipCount)
                                        .Take(archiveChatRequest.PageSize)
                                        .Select(arch => new ArchiveChatItem
                                        {
                                            ConversationId = arch.ConversationId,
                                            Title = arch.Title,
                                            ArchivedAt = arch.UpdatedAt,
                                            MessageCount = arch.Messages.Count(),
                                            Preview = arch.Messages.Where(m => !m.IsDeleted)
                                                                    .Select(m => m.Content)
                                                                    .FirstOrDefault() ?? "No Messages yet"
                                        }).ToListAsync(cancellationToken);

            return new PagedResponse<ArchiveChatItem>
            {
                Items = paginatedData,
                TotalCount = totalCount,
                Page = archiveChatRequest.Page,
                PageSize = archiveChatRequest.PageSize
            };
        }
    }
}
