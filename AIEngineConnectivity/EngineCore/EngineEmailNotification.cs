namespace AIEngineConnectivity.EngineCore
{
    using AIEngineConnectivity.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    public class EmailPayload
    {
        public string ToAddress { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public EngineEvents Template { get; init; }
        public Dictionary<string, string> parameters { get; init; } = [];
    }
}