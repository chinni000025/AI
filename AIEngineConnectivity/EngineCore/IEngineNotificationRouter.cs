using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineNotificationRouter
    {
        public Type EngineNotificationType { get; }
        public ValueTask RouteAsync(EngineNotificationMessage notification, CancellationToken cancellationToken = default);
    }
}
