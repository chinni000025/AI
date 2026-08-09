namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EngineNotification
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; }
        public string NotificationData { get; set; }
        public string NotificationStatus { get; set; }
        public DateTime? LastRetryAt { get; set; }
        public DateTime? RetryAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
