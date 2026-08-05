namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IEmbeddedResourceProvider
    {
        Task<string> GetResourceAsync(string ResourceName, CancellationToken cancellationToken = default);
    }
}
