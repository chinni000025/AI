namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface ITemplateProvider
    {
        Task<string> GetTemplate(EngineEvents @event, CancellationToken ct = default);
    }
}