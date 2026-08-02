using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineBus
    {
        public ValueTask ConnectEngineBus(EngineEvents Event, CancellationToken ct = default);
    }
}