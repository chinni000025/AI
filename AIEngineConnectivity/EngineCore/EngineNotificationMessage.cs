namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public sealed class EngineNotificationMessage
    {
        public required EngineEvents EngineEvents { get; set; }
        public required INotification Notification { get; set; }
    }
}
