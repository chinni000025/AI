namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public sealed class EngineNotification
    {
        public required EngineEvents EngineEvents { get; set; }
        public required INotification Notification { get; set; }
    }
}
