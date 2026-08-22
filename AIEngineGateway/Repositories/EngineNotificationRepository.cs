using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.Repositories
{
    public class EngineNotificationRepository : IEngineNotificationRepository
    {
        private EngineContext _engineContext;

        public EngineNotificationRepository(EngineContext engineContext)
        {
            _engineContext = engineContext;
        }

        public async Task<List<EngineNotificationEvent>> GetNotificationByPriority(CancellationToken cancellationToken)
        {
            var query = await (from e in _engineContext.EngineNotificationEvents
                               orderby e.Priority descending
                               select e).ToListAsync(cancellationToken);
            return query;
        }
    }
}
