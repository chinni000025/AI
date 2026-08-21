using AIEngineConnectivity.EngineCore;
using System;
using System.Reflection;

namespace AIEngineCore.EngineCore
{
    public class EmbeddedResourceProvider : IEmbeddedResourceProvider
    {
        private readonly Assembly _Assembly;
        public EmbeddedResourceProvider(Assembly assembly)
        {
            _Assembly = assembly;
        }
        public async Task<string> GetResourceAsync(string ResourceName,
            CancellationToken cancellationToken = default)
        {
            using var stream = _Assembly.GetManifestResourceStream(ResourceName);
            if (stream is null)
                throw new FileNotFoundException(ResourceName);
            using var render = new StreamReader(stream);
            return await render.ReadToEndAsync(cancellationToken);
        }
    }
}
