using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Models
{
    public class ServerRateLimiterOptions
    {
        public int InitialCapacity { get; set; }
        public int RefillIntervalSeconds { get; set; }
    }
}
