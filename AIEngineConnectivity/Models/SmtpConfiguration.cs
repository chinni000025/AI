namespace AIEngineConnectivity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    public class SmtpConfiguration
    {
        public string Host { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public bool EnableSSL { get; set; } = false;
    }
}