using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.Entities
{
#nullable disable
    public class EngineNotificationEvent
    {
        public Guid Id { get; set; }
        public string EventData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
