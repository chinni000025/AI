using AIEngineConnectivity.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineNotificationRepository
    {
        public Task<List<EngineNotificationEvent>> GetNotificationByPriority(CancellationToken cancellationToken);
        public Task<NotificationRetryAndStatus?> GetNotificationRetryAndStatusAsync(Guid notificationId, CancellationToken cancellationToken);
    }
}
