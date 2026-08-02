namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Concurrent;

    public class EngineNotificationRegistry : IEngineNotificationRegistry
    {
        private readonly ConcurrentDictionary<EngineEvents, ConcurrentBag<IEngineNotification>> _EventMap =
            new();

        public IReadOnlyDictionary<EngineEvents, IReadOnlyCollection<IEngineNotification>> EventMap =>
            _EventMap.ToDictionary(
                    e => e.Key,
                    e => (IReadOnlyCollection<IEngineNotification>)e.Value.ToList().AsReadOnly());

        public EngineNotificationRegistry(IEngineNotificationProvider engineNotificationProvider)
        {
            engineNotificationProvider.RegisterNotification(this);
        }

        public void AddOrUpdateNotification(EngineEvents @event, IEnumerable<IEngineNotification> notifications)
        {
            ArgumentNullException.ThrowIfNull(@event);
            ArgumentNullException.ThrowIfNull(notifications);
            foreach (var notification in notifications)
            {
                addOrUpdateNotifications(@event, notification);
            }
        }

        public void addOrUpdateNotifications(EngineEvents @event, IEngineNotification notification)
        {
            _EventMap.AddOrUpdate(
                key: @event,
                addValueFactory: _ => new ConcurrentBag<IEngineNotification> { notification },
                updateValueFactory: (_, exitingNotificaion) =>
                {
                    exitingNotificaion.Add(notification);
                    return exitingNotificaion;
                });
        }

        public IEnumerable<IEngineNotification> GetNotifications(EngineEvents @event)
        {
            if (_EventMap.TryGetValue(@event, out var engineNotifications))
            {
                return engineNotifications;
            }
            return Enumerable.Empty<IEngineNotification>();
        }
    }
}