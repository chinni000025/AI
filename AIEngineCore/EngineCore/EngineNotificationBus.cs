using AIEngineConnectivity.EngineCore;
using AIEngineCore.EngineNotifications;

namespace AIEngineCore.EngineCore
{
    public class EngineNotificationBus : IEngineBus
    {
        private readonly IEngineQueue<EngineNotificationMessage> _EngineQueue;
        public EngineNotificationBus(IEngineQueue<EngineNotificationMessage> engineQueue)
        {
            _EngineQueue = engineQueue;
        }

        public async ValueTask RouteAsync(EngineNotificationMessage @event, CancellationToken ct = default)
        {
            await _EngineQueue.publishAsync(@event, priority: @event.NotificationPriority, cancellationToken: ct);
        }
    }
}