namespace AIEngineConnectivity.Entities
{
#nullable disable
    public class EngineNotificationEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string EventData { get; set; }
        public bool IsRetriedEvent { get; set; } = false;
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
