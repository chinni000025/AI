namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    public interface IEngineNotificationRegistry
    {
        ConcurrentDictionary<string, ConcurrentBag<IEngineNotification>> EventMap { get; }
        public void addOrUpdateNotifications(string Event, IEngineNotification Notification);
        public IEnumerable<IEngineNotification> GetNotifications(string Event);
    }
}