using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class ConversationRecycleHandler(IConversationRepository conversationRepository) : IRecycleItemHandler
    {
        public async Task DeletePermanently(Guid id, CancellationToken cancellation)
        {
            await conversationRepository.DeleteConversationAsync(id, cancellation);
        }
    }
}