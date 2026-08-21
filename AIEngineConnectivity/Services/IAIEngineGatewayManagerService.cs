using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IAIEngineGatewayManagerService
    {
        Task<bool> StartGatewayAsync(string gatewayDirectory);
        Task<bool> StopGatewayAsync();
        bool IsGatewayRunning();
        Task<bool> IsGatewayHealthyAsync(string healthUrl = "http://localhost:5000");
    }
}
