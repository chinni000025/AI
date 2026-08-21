using AIEngineConnectivity.Constants;
using AIEngineConnectivity.EngineCore;

namespace AIEngineCore.EngineCore
{
    public class TemplateProvider : ITemplateProvider
    {
        private IEmbeddedResourceProvider _EmbeddedResourceProvider;

        public TemplateProvider(IEmbeddedResourceProvider embeddedResourceProvider)
        {
            _EmbeddedResourceProvider = embeddedResourceProvider;
        }
        public Task<string> GetTemplate(EngineEvents @event, CancellationToken ct = default)
        {
            if (Templates.EmailTemplates.TryGetValue(@event, out var resourcePath))
                return _EmbeddedResourceProvider.GetResourceAsync(resourcePath);

            throw new KeyNotFoundException($"Template for event {@event.Value} not found.");
        }
    }
}
