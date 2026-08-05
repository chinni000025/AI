namespace AIEngineCore.EngineNotifications
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.EngineCore;
    using System.Collections.Generic;

    public class EngineEmailNotification : IEngineNotification
    {
        public string ToAddress { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; }
        public Dictionary<string, string> parameters { get; init; } = [];
    }
}