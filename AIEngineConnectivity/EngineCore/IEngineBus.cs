namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineBus
    {
        public ValueTask ConnectEngineBus(string Event, CancellationToken ct = default);
    }
}