using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.DTOs
{
    public class EngineState
    {
        public bool IsEngineReady { get; set; } = false;
        public bool IsEngineRunning { get; set; } = false;
        public string? ErrorMessage { get; set; } = null;
    }
}
