namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineRetryNotification> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IServiceScopeFactory _ServiceScopeFactory;
        private IEngineNotificationService _EngineNotificationService;
        private IEngineLatch _EngineLatch;

        public EngineEmailRetryWorker(IEngineQueue<EngineRetryNotification> emailQueue,
            IEngineNotificationService engineNoitificationService,
            IEngineLatch engineLatch,
            IOptions<WorkerConfiguration> options, IServiceScopeFactory serviceProvider)
        {
            _EmailQueue = emailQueue;
            _WorkerConfiguration = options.Value;
            _ServiceScopeFactory = serviceProvider;
            _EngineNotificationService = engineNoitificationService;
            _EngineLatch = engineLatch;
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
                try
                {
                    await using var scope = _ServiceScopeFactory.CreateAsyncScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendEmail(retryNotification.EngineNotification, cancellation);
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
                    await AddOrUpdateNotificationAsync(retryNotification, DateTime.UtcNow.Add(delay), cancellation);
                    await Task.Delay(delay, cancellation);
                    await _EmailQueue.publishAsync(retryNotification);
                }
            }
        }

        private async Task AddOrUpdateNotificationAsync(EngineRetryNotification engineRetryNotification,
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
                    NotificationType = "Email Notification",
                    NotificationStatus = "Pending",
                    RetryAt = retryAt,
                    CreatedAt = dateTime,
                    ModifiedAt = dateTime
                };
                await _EngineNotificationService.AddEngineNotificationAsync(engineNotification, cancellationToken);
            }
            else
            {
                var exitingNotification = await _EngineNotificationService
                                            .GetEngineNotificationAsync(engineRetryNotification.EngineNotificationId.Value,
                                            cancellationToken);
                if (exitingNotification is null)
                    throw new Exception($"Notificaion {engineRetryNotification.EngineNotificationId.Value} is not present");
                exitingNotification.ModifiedAt = DateTime.Now;
                exitingNotification.NotificationData = _EngineLatch.Serialize(engineRetryNotification);
                exitingNotification.LastRetryAt = exitingNotification.RetryAt;
                exitingNotification.RetryAt = retryAt;
            }
            await _EngineNotificationService.SaveChangesAsync(cancellationToken);
        }
    }
}
