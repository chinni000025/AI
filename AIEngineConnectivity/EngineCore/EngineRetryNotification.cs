namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineRetryNotification
    {
        public required EngineNotificationMessage EngineNotification { get; set; }
        public required int Retries { get; set; } = 0;
    }
}
