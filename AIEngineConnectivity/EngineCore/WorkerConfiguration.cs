namespace AIEngineConnectivity.EngineCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class WorkerConfiguration
    {
        public int ConsumerCount { get; set; } = 5;
        public int MaxRetries { get; set; } = 5;
    }
}
