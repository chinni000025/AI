namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Collections.Concurrent;

    public class EngineNotificationRegistry : IEngineNotificationRegistry
    {
        public ConcurrentDictionary<string, ConcurrentBag<IEngineNotification>> EventMap =>
            new(StringComparer.OrdinalIgnoreCase);

        public void addOrUpdateNotifications(string Event, IEngineNotification Notification)
        {
            EventMap.AddOrUpdate(
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
            if (EventMap.TryGetValue(Event, out var engineNotifications))
            {
                return engineNotifications;
            }
            return Enumerable.Empty<IEngineNotification>();
        }
    }
}