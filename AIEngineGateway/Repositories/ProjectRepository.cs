using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly EngineContext _EngineContext;
        public ProjectRepository(EngineContext engineContext)
        {
            _EngineContext = engineContext;
        }

        public async Task<List<ProjectDTO>> GetAllProjects(string userId, CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var queryResult = await (from pm in _EngineContext.ProjectMembers
                                     join p in _EngineContext.Projects
                                     on pm.ProjectId equals p.Id
                                     where pm.UserId == requiredUserId
                                     orderby p.CreatedAt descending
                                     select new ProjectDTO
                                     {
                                         ProjectName = p.Name,
                                         Conversations = p.Conversations
                                         .Select(c => new ProjectConversation
                                         {
                                             Title = c.Title,
                                             ConversationId = c.ConversationId
                                         }).ToList()
                                     }).ToListAsync(cancellationToken);
            return queryResult;
        }

        public async Task<List<ConversationDTO>> GetConversationInProject(string userId, Guid projectId, Guid conversationId,
            CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var queryResult = await (from pm in _EngineContext.ProjectMembers
                                     join p in _EngineContext.Projects
                                     on pm.ProjectId equals p.Id
                                     join con in _EngineContext.Conversations
                                     on p.Id equals con.ProjectId
                                     where pm.UserId == requiredUserId
                                        && p.ProjectId == projectId
                                        && con.ConversationId == conversationId
                                     select new ConversationDTO
                                     {
                                         ConversationId = con.ConversationId,
                                         ConversationTitle = con.Title,
                                         LastMessage = con.LastMessageAt,
                                         CreatedAt = con.CreatedAt,
                                         ModelUsed = con.ModelUsed
                                     }).ToListAsync(cancellationToken);
            return queryResult;
        }


        public async Task<ConversationWithMessages?> GetConversationMessage(string userId, Guid ProjectId, Guid conversationId,
            CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var queryResult = await (from pm in _EngineContext.ProjectMembers
                                     join p in _EngineContext.Projects
                                     on pm.ProjectId equals p.Id
                                     join con in _EngineContext.Conversations
                                     on p.Id equals con.ProjectId
                                     where pm.UserId == requiredUserId && p.ProjectId == ProjectId
                                        && con.ConversationId == conversationId && !con.IsDeleted
                                     select new ConversationWithMessages
                                     {
                                         ConversationId = con.ConversationId,
                                         Title = con.Title,
                                         Messages = con.Messages.Where(m => !m.IsDeleted)
                                            .OrderBy(m => m.MessageSentAt)
                                            .Select(m => new ConversationMessages
                                            {
                                                Content = m.Content,
                                                RoleId = m.RoleId,
                                                MessagSentAt = m.MessageSentAt
                                            }).ToList()
                                     }).FirstOrDefaultAsync(cancellationToken);
            return queryResult;
        }
    }
}