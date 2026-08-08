using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace AIEngineCore.Extensions
{
    public static class ShouldRetryEmailNotification
    {
        public static bool CanRetryEmailNotification(this Exception exception)
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
