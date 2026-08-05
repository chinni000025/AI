namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineRetryNotification
    {
        public required EngineNotification EngineNotification { get; set; }
        public required int Retries { get; set; } = 0;
    }
}
