using AIEngineConnectivity.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public sealed class EngineNotificationMessage
    {
        public required EngineEvents EngineEvents { get; set; }
        public Guid? NotificationId { get; set; }
        public int Retries { get; set; } = 0;
        public required INotification Notification { get; set; }
    }
}
