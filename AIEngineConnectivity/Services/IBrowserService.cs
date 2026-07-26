namespace AIEngineConnectivity.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IBrowserService
    {
        Task<bool> OpenWebBrowserAsync(string url);
    }
}
