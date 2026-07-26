namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Services;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    public class AIEngineGatewayManagerService : IAIEngineGatewayManagerService
    {
        private Process? _gatewayProcess;
        private const string GatewayProcessName = "AIEngineGateway";

        public async Task<bool> IsGatewayHealthyAsync(string healthUrl = "http://localhost:5000")
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromMinutes(3) })
                {
                    var response = await client.GetAsync(healthUrl);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsGatewayRunning()
        {
            try
            {
                var processes = Process.GetProcessesByName(GatewayProcessName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public Task<bool> StopGatewayAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    // Kill the tracked process first
                    if (_gatewayProcess != null && !_gatewayProcess.HasExited)
                    {
                        _gatewayProcess.Kill(entireProcessTree: true);
                        _gatewayProcess.WaitForExit(5000);
                        _gatewayProcess = null;
                    }

                    // Also kill any other AIEngineGateway processes running system-wide
                    var processes = Process.GetProcessesByName(GatewayProcessName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill(entireProcessTree: true);
                            process.WaitForExit(5000);
                        }
                        catch
                        {
                            // Process may have already exited
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> StartGatewayAsync(string gatewayDirectory)
        {
            string dllPath = Path.Combine(gatewayDirectory, "AIEngineGateway.dll");
            string exePath = Path.Combine(gatewayDirectory, "AIEngineGateway.exe");

            ProcessStartInfo psi;

            if (File.Exists(exePath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = gatewayDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
            }
            else if (File.Exists(dllPath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{dllPath}\"",
                    WorkingDirectory = gatewayDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
            }
            else
            {
                throw new FileNotFoundException($"Gateway binaries not found at {gatewayDirectory}");
            }

            // Explicitly set the URL so the gateway listens on port 5000
            psi.Environment["ASPNETCORE_URLS"] = InstallerConstants.AppWebUrl;
            psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";

            _gatewayProcess = Process.Start(psi);

            if (_gatewayProcess == null || _gatewayProcess.HasExited)
            {
                throw new InvalidOperationException("Failed to start Gateway process.");
            }

            bool healthy = await WaitForHealthCheckAsync(InstallerConstants.AppWebUrl, TimeSpan.FromSeconds(30));

            if (!healthy)
            {
                if (_gatewayProcess.HasExited)
                {
                    return false;
                }
            }

            return healthy;
        }

        private async Task<bool> WaitForHealthCheckAsync(string url, TimeSpan timeout)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    if (_gatewayProcess != null && _gatewayProcess.HasExited)
                        return false;

                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode) return true;
                }
                catch
                {

                }
                await Task.Delay(1000);
            }
            return false;
        }
    }
}
