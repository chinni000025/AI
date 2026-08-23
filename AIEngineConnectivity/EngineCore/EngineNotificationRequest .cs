using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.EngineCore
{
    public class EngineNotificationRequest
    {
        public required EngineEvents EngineEvents { get; set; }
        public required INotification Notification { get; set; }
        public Priority NotificationPriority { get; set; }
    }
}
