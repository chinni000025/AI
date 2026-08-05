namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Models;
    using AIEngineCore.EngineNotifications;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IEmailService
    {
        public Task SendEmail(EngineNotification engineEmailNotification);
        public Task SendTestMail(SmtpConfiguration smtpConfiguration);
    }
}
