namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class EngineEmailWorker : BackgroundService
    {
        private IEngineQueue<EngineNotificationMessage> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IEngineQueue<EngineNotificationMessage> _EngineRetryQueue;
        private IServiceScopeFactory _ServiceScopeFactory;

        public EngineEmailWorker(IEngineQueue<EngineNotificationMessage> emailQueue, IEngineQueue<EngineNotificationMessage> engineRetryQueue,
            IOptions<WorkerConfiguration> options,
            IServiceScopeFactory serviceProvider)
        {
            _EmailQueue = emailQueue;
            _WorkerConfiguration = options.Value;
            _EngineRetryQueue = engineRetryQueue;
            _ServiceScopeFactory = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = Enumerable.Range(0, _WorkerConfiguration.ConsumerCount)
                        .Select(async _ => await ConsumeAsync(stoppingToken));
            await Task.WhenAll(tasks);
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            await foreach (var notification in _EmailQueue.ReadAsync(cancellationToken))
            {
                await using var scope = _ServiceScopeFactory.CreateAsyncScope();
                var engineNotificationService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
                try
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await engineNotificationService.AddOrUpdateNotificationAsync(notification,
                        NotificationType.EmailNotification,
                        EngineNotificationStatus.Processing, null, null, cancellationToken);

                    await emailService.SendEmail(notification, cancellationToken);
                    await engineNotificationService.NotificationSent(notification.NotificationId.Value, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!ex.CanRetryEmailNotification())
                    {
                        continue;
                    }
                    await engineNotificationService.AddOrUpdateNotificationAsync(notification, NotificationType.EmailNotification,
                        EngineNotificationStatus.RetryScheduled, DateTime.UtcNow, ex.Message, cancellationToken);
                    await _EngineRetryQueue.publishAsync(notification, cancellationToken);
                }
            }
        }
    }
}