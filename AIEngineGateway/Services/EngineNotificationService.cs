namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Services;
    using AIEngineGateway.Repositories;

    public class EngineNotificationService : IEngineNotificationService
    {
        private readonly IRepositoryWrapper _Repository;
        private readonly IEngineLatch _EngineLatch;

        public EngineNotificationService(IRepositoryWrapper repository, IEngineLatch engineLatch)
        {
            _Repository = repository;
            _EngineLatch = engineLatch;
        }

        public async Task AddOrUpdateNotificationAsync(EngineRetryNotification engineRetryNotification,
            NotificationType NotificaionType, string NotificationStatus,
            DateTime retryAt, CancellationToken cancellationToken)
        {
            if (engineRetryNotification.EngineNotificationId is null)
            {
                var NotificationId = Guid.NewGuid();
                engineRetryNotification.EngineNotificationId = NotificationId;
                var dateTime = DateTime.Now;
                var engineNotification = new EngineNotification
                {
                    Id = NotificationId,
                    NotificationData = _EngineLatch.Serialize(engineRetryNotification),
                    NotificationType = NotificaionType.ToString(),
                    NotificationStatus = NotificationStatus,
                    RetryAt = retryAt,
                    CreatedAt = dateTime,
                    ModifiedAt = dateTime
                };
                await _Repository.GetEngineRepo<EngineNotification>().AddAsync(engineNotification, cancellationToken);
            }
            else
            {
                var exitingNotification = await GetEngineNotificationAsync(engineRetryNotification.EngineNotificationId.Value,
                                            cancellationToken);
                if (exitingNotification is null)
                    throw new Exception($"Notificaion {engineRetryNotification.EngineNotificationId.Value} is not present");
                exitingNotification.ModifiedAt = DateTime.Now;
                exitingNotification.NotificationData = _EngineLatch.Serialize(engineRetryNotification);
                exitingNotification.LastRetryAt = exitingNotification.RetryAt;
                exitingNotification.RetryAt = retryAt;
            }
            await _Repository.SaveChangesAsync(cancellationToken);
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
            }
        }
    }
}