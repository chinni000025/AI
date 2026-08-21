using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Models
{
    public class UserRateLimiterOptions
    {
        public int InitialCapacity { get; set; }
        public int LeakIntervalSeconds { get; set; }
        public int InActiveInterval { get; set; }
    }
}
