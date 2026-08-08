namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class EngineEmailWorker : BackgroundService
    {
        private IEngineQueue<EngineNotification> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IEngineQueue<EngineRetryNotification> _EngineRetryQueue;
        private IServiceScopeFactory _ServiceScopeFactory;

        public EngineEmailWorker(IEngineQueue<EngineNotification> emailQueue, IEngineQueue<EngineRetryNotification> engineRetryQueue,
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
                try
                {
                    await using var scope = _ServiceScopeFactory.CreateAsyncScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendEmail(notification, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!ex.CanRetryEmailNotification())
                    {
                        continue;
                    }
                    var engineEmailRetryNotification = new EngineRetryNotification
                    {
                        EngineNotification = notification,
                        Retries = 0
                    };
                    await _EngineRetryQueue.publishAsync(engineEmailRetryNotification, cancellationToken);
                }
            }
        }
    }
}