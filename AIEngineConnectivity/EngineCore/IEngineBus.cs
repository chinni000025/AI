namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    public interface IEngineBus
    {
        public ValueTask ConnectEngineBus(EngineEvents Event, CancellationToken ct = default);
    }
}