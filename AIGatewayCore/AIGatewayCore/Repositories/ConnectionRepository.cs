using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly EngineContext _EngineContext;
        public ConnectionRepository(EngineContext EngineContext)
        {
            _EngineContext = EngineContext;
        }

        public async Task<EngineConnection?> GetConnectionsByUserId(String userId, String connectionType,
            CancellationToken cancellationToken)
        {
            int requiredUserId = int.Parse(userId);
            var query = await (from c in _EngineContext.EngineConnections
                               where c.UserId == requiredUserId && c.ConnectionName == connectionType
                               select c).FirstOrDefaultAsync(cancellationToken);
            return query;
        }
    }
}
