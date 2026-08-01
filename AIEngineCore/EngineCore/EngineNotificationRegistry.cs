namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Concurrent;

    public class EngineNotificationRegistry
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<IEngineNotification>> _EventMap =
            new(StringComparer.OrdinalIgnoreCase);
        public void addOrUpdateNotifications(string Event, IEngineNotification Notification)
        {
            _EventMap.AddOrUpdate(
                key: Event,
                addValueFactory: _ => new ConcurrentBag<IEngineNotification> { Notification },
                updateValueFactory: (_, exitingNotificaion) =>
                {
                    exitingNotificaion.Add(Notification);
                    return exitingNotificaion;
                });
        }

        public IEnumerable<IEngineNotification> GetNotifications(string Event)
        {
            if (_EventMap.TryGetValue(Event, out var engineNotifications))
            {
                return engineNotifications;
            }
            return Enumerable.Empty<IEngineNotification>();
        }
    }
}