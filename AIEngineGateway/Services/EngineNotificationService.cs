using AIEngineConnectivity.Constants;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
#nullable disable
    public class EngineNotificationService : IEngineNotificationService
    {
        private readonly IRepositoryWrapper _Repository;
        private readonly IEngineLatch _EngineLatch;
        private readonly ILogger<EngineNotificationService> _logger;

        public EngineNotificationService(IRepositoryWrapper repository, IEngineLatch engineLatch, ILogger<EngineNotificationService> logger)
        {
            _Repository = repository;
            _EngineLatch = engineLatch;
            _logger = logger;
        }

        public async Task AddOrUpdateNotificationAsync(EngineNotificationMessage engineNotificationMessasge,
            NotificationType NotificaionType, EngineNotificationStatus NotificationStatus,
            DateTime? retryAt, string ErrorMessage, CancellationToken cancellationToken)
        {
            var exitingNotification = await GetEngineNotificationAsync(engineNotificationMessasge.NotificationId.Value,
                                        cancellationToken);
            var UtcNow = DateTime.UtcNow;
            if (exitingNotification is null)
            {
                var engineNotification = new EngineNotification
                {
                    Id = engineNotificationMessasge.NotificationId!.Value,
                    NotificationData = _EngineLatch.Serialize(engineNotificationMessasge),
                    NotificationType = NotificaionType.ToString(),
                    NotificationStatus = NotificationStatus.ToString(),
                    NotificationPriority = engineNotificationMessasge.NotificationPriority,
                    ErrorMessage = ErrorMessage,
                    RetryAt = retryAt,
                    CreatedAt = UtcNow,
                    ModifiedAt = UtcNow,
                };
                await _Repository.GetEngineRepo<EngineNotification>().AddAsync(engineNotification, cancellationToken);
            }
            else
            {
                if (exitingNotification is null)
                    throw new Exception($"Notificaion {engineNotificationMessasge.NotificationId.Value} is not present");
                exitingNotification.ModifiedAt = UtcNow;
                exitingNotification.NotificationData = _EngineLatch.Serialize(engineNotificationMessasge);
                exitingNotification.NotificationStatus = NotificationStatus.ToString();
                exitingNotification.LastRetryAt = exitingNotification.RetryAt;
                exitingNotification.RetryAt = retryAt;
                exitingNotification.ErrorMessage = ErrorMessage;
            }
            await _Repository.SaveChangesAsync(cancellationToken);
        }

        public async Task NotificationSent(Guid engineNotificationId, Guid eventId, CancellationToken cancellation)
        {
            var notification = await GetEngineNotificationAsync(engineNotificationId, cancellation);
            var engineEvent = await GetEngineNotificationEventAsync(eventId, cancellation);
            if (notification is null)
            {

                _logger.LogError("Unable to mark notification as completed. Notification {NotificationId} was not found. EventId: {EventId}",
                    engineNotificationId,
                    eventId);
                throw new InvalidOperationException($"Notification '{engineNotificationId}' was not found.");
            }
            var now = DateTime.UtcNow;
            notification.NotificationStatus = EngineNotificationStatus.Completed.ToString();
            notification.CompletedAt = now;
            notification.ErrorMessage = null;
            notification.ModifiedAt = now;
            if (engineEvent is null)
            {
                _logger.LogError("Unable to mark notification {NotificationId} as completed. Event {EventId} was not found.", engineNotificationId,
                                eventId);
                return;
            }
            engineEvent.EventData = _EngineLatch.Serialize(notification);
            engineEvent.ModifiedAt = now;

            await _Repository.SaveChangesAsync(cancellation);

        }

        public async Task NotificationFailed(Guid notificationId, Guid eventId, string errorMessage, CancellationToken cancellation)
        {
            var notification = await GetEngineNotificationAsync(notificationId, cancellation);
            var engineEvent = await GetEngineNotificationEventAsync(eventId, cancellation);
            if (notification is null)
            {
                _logger.LogError("Unable to mark notification {NotificationId} as Failed because the notification was not found.", notificationId);
                return;
            }
            if (engineEvent is null)
            {
                _logger.LogError("Unable to mark notification {NotificationId} as Failed. Event {EventId} was not found.", notificationId,
                              eventId);
                return;
            }

            notification.NotificationStatus = EngineNotificationStatus.Failed.ToString();
            notification.CompletedAt = null;
            notification.ErrorMessage = errorMessage;
            notification.ModifiedAt = DateTime.UtcNow;
            engineEvent.EventData = _EngineLatch.Serialize(notification);
            engineEvent.ModifiedAt = DateTime.UtcNow;

            await _Repository.SaveChangesAsync(cancellation);
        }

        public async Task NotificationDeadLettered(Guid notificationId, Guid eventId, string errorMessage, CancellationToken cancellationToken)
        {
            var notification = await GetEngineNotificationAsync(notificationId, cancellationToken);
            var engineEvent = await GetEngineNotificationEventAsync(eventId, cancellationToken);
            if (notification is null)
            {
                _logger.LogError($"Unable to mark notification{notificationId} as Dead Lettered because the notification was not found.");
                return;
            }
            if (engineEvent is null)
            {
                _logger.LogError("Unable to mark notification as ... Event was not found.");
                return;
            }
            var now = DateTime.UtcNow;
            notification.NotificationStatus = EngineNotificationStatus.DeadLettered.ToString();
            notification.CompletedAt = null;
            notification.ErrorMessage = errorMessage;
            notification.ModifiedAt = now;
            engineEvent.EventData = _EngineLatch.Serialize(notification);
            engineEvent.ModifiedAt = now;
            await _Repository.SaveChangesAsync(cancellationToken);
        }

        public async Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation)
        {
            return await _Repository.GetEngineRepo<EngineNotification>()
                           .GetByIdAsync(engineNotificationId, cancellation);
        }

        public async Task<EngineNotificationEvent?> GetEngineNotificationEventAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _Repository.GetEngineRepo<EngineNotificationEvent>()
               .GetByIdAsync(eventId, cancellationToken);
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

        public async Task InsertEventNotification(EngineNotificationEvent engineNotificationEvent, CancellationToken cancellationToken)
        {
            await _Repository.GetEngineRepo<EngineNotificationEvent>()
                    .AddAsync(engineNotificationEvent, cancellationToken);
            await _Repository.SaveChangesAsync(cancellationToken);
        }
    }
}