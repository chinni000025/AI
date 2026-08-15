namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;

    public class EngineNotificationService : IEngineNotificationService
    {
        private readonly IRepositoryWrapper _Repository;
        private readonly IEngineLatch _EngineLatch;

        public EngineNotificationService(IRepositoryWrapper repository, IEngineLatch engineLatch)
        {
            _Repository = repository;
            _EngineLatch = engineLatch;
        }

        public async Task AddOrUpdateNotificationAsync(EngineNotificationMessage engineRetryNotification,
            NotificationType NotificaionType, EngineNotificationStatus NotificationStatus,
            DateTime? retryAt, string ErrorMessage, CancellationToken cancellationToken)
        {
            var UtcNow = DateTime.UtcNow;
            if (engineRetryNotification.NotificationId is null)
            {
                var NotificationId = Guid.NewGuid();
                engineRetryNotification.NotificationId = NotificationId;
                var engineNotification = new EngineNotification
                {
                    Id = NotificationId,
                    NotificationData = _EngineLatch.Serialize(engineRetryNotification),
                    NotificationType = NotificaionType.ToString(),
                    NotificationStatus = NotificationStatus.ToString(),
                    RetryAt = retryAt,
                    CreatedAt = UtcNow,
                    ModifiedAt = UtcNow,
                    ErrorMessage = ErrorMessage
                };
                await _Repository.GetEngineRepo<EngineNotification>().AddAsync(engineNotification, cancellationToken);
            }
            else
            {
                var exitingNotification = await GetEngineNotificationAsync(engineRetryNotification.NotificationId.Value,
                                            cancellationToken);
                if (exitingNotification is null)
                    throw new Exception($"Notificaion {engineRetryNotification.NotificationId.Value} is not present");
                exitingNotification.ModifiedAt = UtcNow;
                exitingNotification.NotificationData = _EngineLatch.Serialize(engineRetryNotification);
                exitingNotification.NotificationStatus = NotificationStatus.ToString();
                exitingNotification.LastRetryAt = exitingNotification.RetryAt;
                exitingNotification.RetryAt = retryAt;
                exitingNotification.ErrorMessage = ErrorMessage;
            }
            await _Repository.SaveChangesAsync(cancellationToken);
        }

        public async Task NotificationSent(Guid engineNotificationId, CancellationToken cancellation)
        {
            var notification = await GetEngineNotificationAsync(engineNotificationId, cancellation);
            if (notification is not null)
            {
                notification.NotificationStatus = EngineNotificationStatus.Completed.ToString();
                notification.CompletedAt = DateTime.UtcNow;
                notification.ErrorMessage = null;
                notification.ModifiedAt = DateTime.UtcNow;
                await _Repository.SaveChangesAsync(cancellation);
            }
        }

        public async Task NotificationFailed(Guid notificationId, string errorMessage, CancellationToken cancellation)
        {
            var notification = await GetEngineNotificationAsync(notificationId, cancellation);
            if (notification is not null)
            {
                notification.NotificationStatus = EngineNotificationStatus.Failed.ToString();
                notification.CompletedAt = DateTime.UtcNow;
                notification.ErrorMessage = errorMessage;
                notification.ModifiedAt = DateTime.UtcNow;
                await _Repository.SaveChangesAsync(cancellation);
            }
        }

        public async Task NotificationDeadLettered(Guid notificationId, string errorMessage, CancellationToken cancellationToken)
        {
            var notification = await GetEngineNotificationAsync(notificationId, cancellationToken);
            if (notification is not null)
            {
                notification.NotificationStatus = EngineNotificationStatus.DeadLettered.ToString();
                notification.CompletedAt = DateTime.UtcNow;
                notification.ErrorMessage = errorMessage;
                notification.ModifiedAt = DateTime.UtcNow;
                await _Repository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation)
        {
            return await _Repository.GetEngineRepo<EngineNotification>()
                           .GetByIdAsync(engineNotificationId, cancellation);
        }

        public async Task RemoveEngineNotification(Guid engineNotificationId, CancellationToken cancellationToken)
        {
            var engineNotification = await GetEngineNotificationAsync(engineNotificationId, cancellationToken);
            if (engineNotification is not null)
            {
                _Repository.GetEngineRepo<EngineNotification>().delete(engineNotification);
                await _Repository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}