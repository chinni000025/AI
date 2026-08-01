namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;

    public class EngineEventPublisher<T>
    {
        private readonly IEngineBus _EngineBus;
        public EngineEventPublisher(IEngineBus engineBus)
        {
            _EngineBus = engineBus;
        }
        public async Task PublishEvent(string Event, CancellationToken cancellationToken = default)
        {
            await _EngineBus.ConnectEngineBus(Event, cancellationToken);
        }
    }
}