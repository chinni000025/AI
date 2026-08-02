using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineNotificationRouter
    {
        public Type EngineNotificationType { get; }
        public ValueTask publishAsync(IEngineNotification notification, CancellationToken cancellationToken = default);
    }
}
