using AIEngineConnectivity.EngineCore;
using AIEngineCore.EngineNotifications;

namespace AIEngineCore.EngineCore
{
    public class EngineBus : IEngineBus
    {
        private readonly IEngineQueue<EngineNotificationMessage> _EngineQueue;
        public EngineBus(IEngineQueue<EngineNotificationMessage> engineQueue)
        {
            _EngineQueue = engineQueue;
        }

        public async ValueTask RouteAsync(EngineNotificationMessage @event, CancellationToken ct = default)
        {
            await _EngineQueue.publishAsync(@event, ct);
        }
    }
}