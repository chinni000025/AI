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
                OperationCanceledException => false,
                NotSupportedException => false,
                ArgumentException => false,
                FormatException => false,
                TimeoutException => true,
                SocketException => true,
                SmtpException smtp => smtp.StatusCode switch
                {
                    SmtpStatusCode.MailboxBusy => true,
                    SmtpStatusCode.MailboxUnavailable => true,
                    SmtpStatusCode.TransactionFailed => true,
                    SmtpStatusCode.InsufficientStorage => true,
                    SmtpStatusCode.ServiceNotAvailable => true,
                    _ => false
                },
                _ => false
            };
        }
    }
}
