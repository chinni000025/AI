using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEmbeddedResourceProvider
    {
        Task<string> GetResourceAsync(string ResourceName, CancellationToken cancellationToken = default);
    }
}
