namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public class EngineEmailWorker : BackgroundService
    {
        private IEngineQueue<EngineNotification> _EmailQueue;
        private IEmailService _EmailService;
        private WorkerConfiguration _WorkerConfiguration;
        private IEngineQueue<EngineRetryNotification> _EngineRetryQueue;

        public EngineEmailWorker(IEngineQueue<EngineNotification> emailQueue,
            IEmailService emailService, IEngineQueue<EngineRetryNotification> engineRetryQueue,
            IOptions<WorkerConfiguration> options)
        {
            _EmailQueue = emailQueue;
            _EmailService = emailService;
            _WorkerConfiguration = options.Value;
            _EngineRetryQueue = engineRetryQueue;
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
                    await _EmailService.SendEmail(notification, cancellationToken);
                }
                catch
                {
                    var engineEmailRetryNotification = new EngineRetryNotification
                    {
                        EngineNotification = notification,
                        Retries = 1
                    };
                    await _EngineRetryQueue.publishAsync(engineEmailRetryNotification, cancellationToken);
                }
            }
        }
    }
}