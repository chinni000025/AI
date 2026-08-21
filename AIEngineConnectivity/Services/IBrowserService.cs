using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IBrowserService
    {
        Task<bool> OpenWebBrowserAsync(string url);
    }
}
