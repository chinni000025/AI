namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
#nullable disable
    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineNotificationMessage> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IServiceScopeFactory _ServiceScopeFactory;

        public EngineEmailRetryWorker(IEngineQueue<EngineNotificationMessage> emailQueue,
            IOptions<WorkerConfiguration> options, IServiceScopeFactory serviceProvider)
        {
            _EmailQueue = emailQueue;
            _WorkerConfiguration = options.Value;
            _ServiceScopeFactory = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var tasks = Enumerable.Range(0, _WorkerConfiguration.ConsumerCount)
                                .Select(async _ => await SendEmail(stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        }

        public async Task SendEmail(CancellationToken cancellation)
        {
            await foreach (var retryNotification in _EmailQueue.ReadAsync(cancellation))
            {
                await using var scope = _ServiceScopeFactory.CreateAsyncScope();
                var engineNotificationService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
                try
                {
                    await engineNotificationService.AddOrUpdateNotificationAsync(retryNotification, NotificationType.EmailNotification,
                        EngineNotificationStatus.Processing, null, null, cancellation);
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendEmail(retryNotification, cancellation);
                    await engineNotificationService.NotificationSent(retryNotification.NotificationId.Value, cancellation);
                }
                catch (Exception ex)
                {
                    if (!ex.CanRetryEmailNotification())
                    {
                        //logging or some thing.
                        continue;
                    }
                    retryNotification.Retries++;
                    if (retryNotification.Retries > _WorkerConfiguration.MaxRetries)
                    {
                        //Dead letter Queue
                        continue;
                    }
                    var delay = retryNotification.Retries.GetExponentialBackoff();
                    await engineNotificationService.AddOrUpdateNotificationAsync(retryNotification, NotificationType.EmailNotification,
                        EngineNotificationStatus.RetryScheduled, DateTime.UtcNow.Add(delay), ex.Message, cancellation);

                    var scheduler = scope.ServiceProvider.GetRequiredService<IEngineScheduler>();
                    await scheduler.ScheduleEngineNotification(new ScheduleEngineNotificationDTO
                    {
                        NotificationType = NotificationType.EmailNotification,
                        RetryAt = DateTime.UtcNow.Add(delay),
                        NotificationId = retryNotification.NotificationId.Value
                    }, cancellation);

                    await _EmailQueue.publishAsync(retryNotification);
                }
            }
        }
    }
}