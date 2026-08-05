namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface IEngineBus
    {
        public ValueTask RouteAsync(EngineNotification Event, CancellationToken ct = default);
    }
}