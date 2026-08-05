namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface ITemplateProvider
    {
        Task<string> GetTemplate(EngineNotification @event, CancellationToken ct = default);
    }
}