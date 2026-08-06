namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using System.Net.Mail;
    using System.Net.Sockets;

    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineRetryNotification> _EmailQueue;
        private IEmailService _EmailService;
        private WorkerConfiguration _WorkerConfiguration;
        public EngineEmailRetryWorker(IEngineQueue<EngineRetryNotification> emailQueue,
            IEmailService emailService, IOptions<WorkerConfiguration> options)
        {
            _EmailQueue = emailQueue;
            _EmailService = emailService;
            _WorkerConfiguration = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = Enumerable.Range(0, _WorkerConfiguration.ConsumerCount)
                            .Select(async _ => await SendEmail(stoppingToken));
            await Task.WhenAll(tasks);
        }

        public async Task SendEmail(CancellationToken cancellation)
        {
            await foreach (var retryNotification in _EmailQueue.ReadAsync(cancellation))
            {
                try
                {
                    await _EmailService.SendEmail(retryNotification.EngineNotification, cancellation);
                }
                catch (Exception ex)
                {
                    if (!ShouldRetry(ex))
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
        private static bool ShouldRetry(Exception exception)
        {
            return exception switch
            {
                TimeoutException => true,
                SocketException => true,
                SmtpException smtp
                    when smtp.StatusCode == SmtpStatusCode.MailboxBusy
                      || smtp.StatusCode == SmtpStatusCode.MailboxUnavailable
                      || smtp.StatusCode == SmtpStatusCode.TransactionFailed => true,
                _ => false
            };
        }
    }
}
