using AIEngineConnectivity.Constants;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Services;
using AIEngineCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEngineCore.EngineNotifications
{
# nullable disable
    public class EngineEmailWorker : BackgroundService
    {
        private IEngineQueue<EngineNotificationMessage> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IEngineQueue<EngineNotificationMessage> _EngineRetryQueue;
        private IServiceScopeFactory _ServiceScopeFactory;
        private readonly ILogger<EngineEmailWorker> _Logger;

        public EngineEmailWorker(IEngineQueue<EngineNotificationMessage> emailQueue, IEngineQueue<EngineNotificationMessage> engineRetryQueue,
            IOptions<WorkerConfiguration> options, ILogger<EngineEmailWorker> logger,
            IServiceScopeFactory serviceProvider)
        {
            _EmailQueue = emailQueue;
            _WorkerConfiguration = options.Value;
            _EngineRetryQueue = engineRetryQueue;
            _ServiceScopeFactory = serviceProvider;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("EngineEmailWorker starting with {ConsumerCount} consumers.", _WorkerConfiguration.ConsumerCount);
            try
            {
                var tasks = Enumerable.Range(0, _WorkerConfiguration.ConsumerCount)
                        .Select(async _ => await ConsumeAsync(stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _Logger.LogInformation("EngineEmailWorker is stopping gracefully due to host shutdown");
            }
            catch (Exception ex)
            {
                _Logger.LogCritical($"Unhandled Exception was caugth in Email worker : {ex}");
            }
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
                        NotificationType.EmailNotification, EngineNotificationStatus.Processing, null, null, cancellationToken);

                    await emailService.SendEmail(notification, cancellationToken);
                    await engineNotificationService.NotificationSent(notification.NotificationId.Value, cancellationToken);
                    _Logger.LogInformation("Notification {NotificationId} successfully processed and marked Completed.", notification.NotificationId.Value);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _Logger.LogWarning("Cancellation triggered during processing for {NotificationId}. Leaving for restart recovery.", notification.NotificationId);
                    throw;
                }
                catch (Exception ex)
                {
                    if (ex.CanRetryEmailNotification())
                    {
                        notification.Retries = 1;
                        var delay = notification.Retries.GetExponentialBackoff();
                        await engineNotificationService.AddOrUpdateNotificationAsync(notification, NotificationType.EmailNotification,
                            EngineNotificationStatus.RetryScheduled, DateTime.UtcNow.Add(delay), ex.Message, cancellationToken);

                        var scheduler = scope.ServiceProvider.GetRequiredService<IEngineScheduler>();
                        await scheduler.ScheduleNotification(notification, DateTime.UtcNow.Add(delay), NotificationType.EmailNotification,
                            cancellationToken);

                        _Logger.LogInformation("Scheduled retry #1 for notification {NotificationId} at {RetryAt} (delay: {DelaySeconds}s).",
                           notification.NotificationId.Value, DateTime.UtcNow.Add(delay), delay.TotalSeconds);
                        await _EngineRetryQueue.publishAsync(notification, cancellationToken);
                        continue;
                    }

                    _Logger.LogError($"Can't Retry Notification with Notification Id {notification.NotificationId.Value} with Exception : " + ex.Message);
                    if (notification.NotificationId.HasValue)
                    {
                        await engineNotificationService.NotificationDeadLettered(notification.NotificationId.Value,
                            ex.Message, cancellationToken);
                    }
                }
            }
        }
    }
}