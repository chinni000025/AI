using AIEngineConnectivity.Constants;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Services;
using Quartz;

namespace AIEngineGateway.BackgroundServices.Jobs
{
    public class EngineNotificationJob : IJob
    {
        private readonly IServiceScopeFactory _ServiceScopeFactory;
        private readonly ILogger<EngineNotificationJob> _Logger;
        public EngineNotificationJob(IServiceScopeFactory serviceScopeFactory, ILogger<EngineNotificationJob> logger)
        {
            _ServiceScopeFactory = serviceScopeFactory;
            _Logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = _ServiceScopeFactory.CreateAsyncScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
            var notificationIdRaw = context.MergedJobDataMap.GetString("NotificationId");
            if (!Guid.TryParse(notificationIdRaw, out var notificationId))
            {
                _Logger.LogError("EngineNotificationJob received invalid NotificationId: '{RawId}'", notificationIdRaw);
                return;
            }
            var notification = await notificationService.GetEngineNotificationAsync(notificationId, context.CancellationToken);
            if (notification == null)
            {
                _Logger.LogWarning("Notification {NotificationId} not found in database.", notificationId);
                return;
            }

            if (notification?.NotificationStatus != EngineNotificationStatus.RetryScheduled.ToString())
            {
                _Logger.LogInformation("Notification {NotificationId} is currently '{Status}'. Skipping enqueue.",
                    notificationId, notification?.NotificationStatus);
                return;
            }

            var engineLatch = scope.ServiceProvider.GetRequiredService<IEngineLatch>();
            var notificationData = engineLatch.Deserialize<EngineNotificationMessage>(notification.NotificationData);
            var retryQueue = scope.ServiceProvider.GetRequiredService<IEngineQueue<EngineNotificationMessage>>();
            await retryQueue.publishAsync(notificationData);
            _Logger.LogInformation("Successfully re-enqueued Notification {NotificationId} for processing.", notificationId);
        }
    }
}
