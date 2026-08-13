namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineNotificationService
    {
        public Task AddOrUpdateNotificationAsync(EngineRetryNotification engineRetryNotification,
            NotificationType NotificaionType, string NotificationStatus,
            DateTime retryAt, CancellationToken cancellationToken);
        public Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation);
        public Task RemoveEngineNotification(Guid engineNotificationId, CancellationToken cancellationToken);
    }
}
