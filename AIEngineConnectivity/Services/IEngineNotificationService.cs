namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using System;

    public interface IEngineNotificationService
    {
        public Task AddOrUpdateNotificationAsync(EngineNotificationMessage engineRetryNotification,
            NotificationType NotificaionType, EngineNotificationStatus NotificationStatus,
            DateTime? retryAt, string? ErrorMessage, CancellationToken cancellationToken);
        public Task NotificationSent(Guid engineNotificationId, CancellationToken cancellation);
        public Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation);
        public Task RemoveEngineNotification(Guid engineNotificationId, CancellationToken cancellationToken);
    }
}
