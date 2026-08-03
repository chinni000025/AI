namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineNotificationRouter
    {
        public Type EngineNotificationType { get; }
        public ValueTask publishAsync(IEngineNotification notification, CancellationToken cancellationToken = default);
    }
}
