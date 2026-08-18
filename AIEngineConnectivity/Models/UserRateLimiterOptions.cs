namespace AIEngineConnectivity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class UserRateLimiterOptions
    {
        public int InitialCapacity { get; set; }
        public int RefillIntervalSeconds { get; set; }
    }
}
