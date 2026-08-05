namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineCore.EngineNotifications;

    public class EngineBus : IEngineBus
    {
        private readonly EngineNotificationRegistry _EngineNotificationRegistry;
        private readonly IEngineQueue<IEngineNotification> _EngineQueue;
        public EngineBus(EngineNotificationRegistry engineNotificationRegistry,
            IEngineQueue<IEngineNotification> engineQueue)
        {
            _EngineNotificationRegistry = engineNotificationRegistry;
            _EngineQueue = engineQueue;
        }

        public async ValueTask RouteAsync(EngineNotification @event, CancellationToken ct = default)
        {
            var notifications = _EngineNotificationRegistry.GetNotifications(@event.EngineEvents);
            foreach (var notification in notifications)
            {
                await _EngineQueue.publishAsync(notification, ct);
            }
        }
    }
}