namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;

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

        public async ValueTask ConnectEngineBus(EngineEvents @event, CancellationToken ct = default)
        {
            var notifications = _EngineNotificationRegistry.GetNotifications(@event);
            foreach (var notification in notifications)
            {
                await _EngineQueue.publishAsync(notification, ct);
            }
        }
    }
}