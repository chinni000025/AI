namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Services;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    public class WindowsBrowserService : IBrowserService
    {
        public Task<bool> OpenWebBrowserAsync(string url)
        {
            return Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
