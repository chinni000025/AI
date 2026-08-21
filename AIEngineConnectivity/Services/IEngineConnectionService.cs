using AIEngineConnectivity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineConnectionService
    {
        public Task GoogleConnectionAuthorizationCode(string code, string userId, CancellationToken cancellationToken);
        public Task SaveAndConnectGoogleConnection(string clientId, string clientSecret, string scheme, string host,
                                                   CancellationToken cancellationToken);
        public Task SaveSmtpConfiguration(SmtpConfiguration smtpConfiguration, CancellationToken cancellationToken);
    }
}
