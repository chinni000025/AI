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

        public EngineEmailWorker(IEngineQueue<EngineNotification> emailQueue,
            IEmailService emailService,
            IOptions<WorkerConfiguration> options)
        {
            _EmailQueue = emailQueue;
            _EmailService = emailService;
            _WorkerConfiguration = options.Value;
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
                //Process overhere.
            }
        }
    }
}