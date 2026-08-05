namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface IEngineBus
    {
        public ValueTask RouteAsync(EngineEvents Event, CancellationToken ct = default);
    }
}