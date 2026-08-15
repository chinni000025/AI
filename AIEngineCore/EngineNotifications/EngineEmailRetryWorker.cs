namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
#nullable disable
    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineNotificationMessage> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IServiceScopeFactory _ServiceScopeFactory;
        private ILogger<EngineEmailRetryWorker> _Logger;

        public EngineEmailRetryWorker(IEngineQueue<EngineNotificationMessage> emailQueue, ILogger<EngineEmailRetryWorker> logger,
            IOptions<WorkerConfiguration> options, IServiceScopeFactory serviceProvider)
        {
            _EmailQueue = emailQueue;
            _WorkerConfiguration = options.Value;
            _ServiceScopeFactory = serviceProvider;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("EngineEmailRetryWorker starting with {ConsumerCount} consumers.", _WorkerConfiguration.ConsumerCount);
            try
            {
                var tasks = Enumerable.Range(0, _WorkerConfiguration.ConsumerCount)
                                .Select(async _ => await ConsumeAsync(stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _Logger.LogInformation("EngineEmailRetryWorker is stopping gracefully due to host shutdown");
            }
            catch (Exception ex)
            {
                _Logger.LogCritical($"Unhandled Exception was caugth in Email worker : {ex}");
            }
        }

        public async Task ConsumeAsync(CancellationToken cancellation)
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

                    var existingNotification = await engineNotificationService.GetEngineNotificationAsync(retryNotification.NotificationId.Value, cancellation);

                    if (existingNotification.NotificationStatus !=
                        EngineNotificationStatus.Completed.ToString()
                        || existingNotification.NotificationStatus !=
                        EngineNotificationStatus.DeadLettered.ToString())
                    {
                        await emailService.SendEmail(retryNotification, cancellation);

                        await engineNotificationService.NotificationSent(retryNotification.NotificationId.Value, cancellation);
                        _Logger.LogInformation($"Notification {retryNotification.NotificationId.Value} successfully processed On Retry Count{retryNotification.Retries} and marked Completed.");
                    }

                }
                catch (Exception ex)
                {
                    if (ex.CanRetryEmailNotification())
                    {
                        retryNotification.Retries++;
                        if (retryNotification.Retries > _WorkerConfiguration.MaxRetries)
                        {
                            await engineNotificationService.NotificationDeadLettered(retryNotification.NotificationId.Value,
                                "Maximum Retries Reached", cancellation);
                            continue;
                        }
                        var delay = retryNotification.Retries.GetExponentialBackoff();
                        await engineNotificationService.AddOrUpdateNotificationAsync(retryNotification, NotificationType.EmailNotification,
                            EngineNotificationStatus.RetryScheduled, DateTime.UtcNow.Add(delay), ex.Message, cancellation);

                        var scheduler = scope.ServiceProvider.GetRequiredService<IEngineScheduler>();
                        await scheduler.ScheduleNotification(retryNotification, DateTime.UtcNow.Add(delay),
                            NotificationType.EmailNotification, cancellation);
                        continue;
                    }
                    _Logger.LogError($"Can't Retry Notification with Notification Id {retryNotification.NotificationId.Value} with Exception : " + ex.Message);
                    if (retryNotification.NotificationId is not null)
                    {
                        await engineNotificationService.NotificationDeadLettered(retryNotification.NotificationId.Value,
                            EngineNotificationStatus.Failed.ToString(), cancellation);
                    }
                }
            }
        }
    }
}