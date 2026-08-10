namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineNotificationRouter
    {
        public Type EngineNotificationType { get; }
        public ValueTask publishAsync(EngineNotificationMessage notification, CancellationToken cancellationToken = default);
    }
}
