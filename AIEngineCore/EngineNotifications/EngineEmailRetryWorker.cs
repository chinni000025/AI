namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using System.Net.Mail;
    using System.Net.Sockets;

    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineRetryNotification> _EmailQueue;
        private WorkerConfiguration _WorkerConfiguration;
        private IServiceScopeFactory _ServiceScopeFactory;
        public EngineEmailRetryWorker(IEngineQueue<EngineRetryNotification> emailQueue,
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
                    await Task.Delay(delay, cancellation);
                    await _EmailQueue.publishAsync(retryNotification);
                }
            }
        }
    }
}
