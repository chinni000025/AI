namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineRetryNotification
    {
        public Guid? EngineNotificationId { get; set; }
        public required EngineNotificationMessage EngineNotification { get; set; }
        public required int Retries { get; set; } = 0;
    }
}
