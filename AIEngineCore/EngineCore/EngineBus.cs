namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineCore.EngineNotifications;

    public class EngineBus : IEngineBus
    {
        private readonly IEngineQueue<EngineNotification> _EngineQueue;
        public EngineBus(IEngineQueue<EngineNotification> engineQueue)
        {
            _EngineQueue = engineQueue;
        }

        public async ValueTask RouteAsync(EngineNotification @event, CancellationToken ct = default)
        {
            await _EngineQueue.publishAsync(@event, ct);
        }
    }
}