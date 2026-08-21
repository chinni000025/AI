using AIEngineConnectivity.Constants;
using AIEngineConnectivity.EngineCore;
using System.Collections.Generic;

namespace AIEngineCore.EngineNotifications
{
    public class EngineEmailNotification : INotification
    {
        public string ToAddress { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public Dictionary<string, string> parameters { get; init; } = [];
    }
}