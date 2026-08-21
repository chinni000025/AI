using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Models;
using AIEngineCore.EngineNotifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEmailService
    {
        public Task SendEmail(EngineNotificationMessage engineEmailNotification, CancellationToken cancellation);
        public Task SendTestMail(SmtpConfiguration smtpConfiguration);
    }
}
