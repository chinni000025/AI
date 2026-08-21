using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineBus
    {
        public ValueTask RouteAsync(EngineNotificationMessage Event, CancellationToken ct = default);
    }
}