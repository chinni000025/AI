namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;

    public class EngineEventPublisher
    {
        private readonly IEngineBus _EngineBus;
        public EngineEventPublisher(IEngineBus engineBus)
        {
            _EngineBus = engineBus;
        }
        public async Task PublishEvent(EngineNotificationMessage @event, CancellationToken cancellationToken = default)
        {
            await _EngineBus.RouteAsync(@event, cancellationToken);
        }
    }
}