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
                               select e).ToListAsync(cancellationToken);
            return query;
        }

        public async Task<NotificationRetryAndStatus?> GetNotificationRetryAndStatusAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            var query = await (from n in _engineContext.EngineNotifications
                               where n.Id == notificationId
                               select new NotificationRetryAndStatus
                               {
                                   NotificationStatus = n.NotificationStatus,
                                   Retries = n.Retries
                               }).FirstOrDefaultAsync(cancellationToken);
            return query;
        }
    }
}
