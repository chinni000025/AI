using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineCore.EngineNotifications
{
    public class EngineEmailRetryWorker : BackgroundService
    {
        private IEngineQueue<EngineRetryNotification> _EmailQueue;
        private IEmailService _EmailService;
        private WorkerConfiguration _WorkerConfiguration;
        public EngineEmailRetryWorker(IEngineQueue<EngineRetryNotification> emailQueue,
            IEmailService emailService,
            IOptions<WorkerConfiguration> options)
        {
            _EmailQueue = emailQueue;
            _EmailService = emailService;
            _WorkerConfiguration = options.Value;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
