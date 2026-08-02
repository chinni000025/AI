namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface INotificationRouter
    {
        public Type NotificationType { get; }
        public ValueTask RouterAsync(IEngineNotification notification, CancellationToken cancellationToken = default);
    }
}
