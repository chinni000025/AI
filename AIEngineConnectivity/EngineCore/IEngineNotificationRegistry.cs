namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    public interface IEngineNotificationRegistry
    {
        IReadOnlyDictionary<EngineEvents, IReadOnlyCollection<IEngineNotification>> EventMap { get; }
        public void addOrUpdateNotifications(EngineEvents @event, IEngineNotification notification);
        public void AddOrUpdateNotification(EngineEvents @Events, IEnumerable<IEngineNotification> notifications);
        public IEnumerable<IEngineNotification> GetNotifications(EngineEvents @event);
    }
}