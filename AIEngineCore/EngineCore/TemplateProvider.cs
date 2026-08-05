namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    public class TemplateProvider : ITemplateProvider
    {
        private IEmbeddedResourceProvider _EmbeddedResourceProvider;

        public TemplateProvider(IEmbeddedResourceProvider embeddedResourceProvider)
        {
            _EmbeddedResourceProvider = embeddedResourceProvider;
        }
        public Task<string> GetTemplate(EngineEvents @event, CancellationToken ct = default)
        {
            return _EmbeddedResourceProvider.GetResourceAsync(@event.Value);
        }
    }
}
