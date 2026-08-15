namespace AIEngineGateway.BackgroundServices.Jobs
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using Quartz;

    public class EngineNotificationJob : IJob
    {
        private readonly IServiceScopeFactory _ServiceScopeFactory;
        public EngineNotificationJob(IServiceScopeFactory serviceScopeFactory)
        {
            _ServiceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await using var scope = _ServiceScopeFactory.CreateAsyncScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
            var notificationId = context.MergedJobDataMap.GetGuid("NotificationId");
            var notification = await notificationService.GetEngineNotificationAsync(notificationId, context.CancellationToken);
            if (notification is not null)
            {
                var engineLatch = scope.ServiceProvider.GetRequiredService<IEngineLatch>();
                var notificationData = engineLatch.Deserialize<EngineNotificationMessage>(notification.NotificationData);
                var retryQueue = scope.ServiceProvider.GetRequiredService<IEngineQueue<EngineNotificationMessage>>();
                await retryQueue.publishAsync(notificationData);
            }
        }
    }
}
