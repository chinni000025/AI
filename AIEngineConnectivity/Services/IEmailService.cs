using AIEngineConnectivity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEmailService
    {
        public Task SendEmail(string to, string body);
        public Task SendTestMail(SmtpConfiguration smtpConfiguration);
    }
}
