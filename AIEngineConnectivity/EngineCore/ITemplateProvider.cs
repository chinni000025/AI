using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.EngineCore
{
    public interface ITemplateProvider
    {
        Task<string> GetTemplate(EngineEvents @event, CancellationToken ct = default);
    }
}