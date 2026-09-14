using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.BackgroundServices.Jobs
{
#nullable disable
    public class DeleteConversationsJob : ICleanUpJob //Needs to use Quartz.
    {
        public async Task ExecuteAsync(EngineContext engineContext , CancellationToken cancellationToken)
        {
            var timeBuffer = DateTime.UtcNow.AddHours(-24);
            var conversation = await engineContext.Conversations.Where(c => c.IsDeleted && timeBuffer < c.UpdatedAt)
                .Take(50).ToListAsync(cancellationToken);

            if (conversation.Count > 0)
            {
                engineContext.Conversations.RemoveRange(conversation);
                await engineContext.SaveChangesAsync();
            }
        }
    }
}
