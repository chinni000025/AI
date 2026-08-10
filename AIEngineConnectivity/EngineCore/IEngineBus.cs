namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface IEngineBus
    {
        public ValueTask RouteAsync(EngineNotificationMessage Event, CancellationToken ct = default);
    }
}