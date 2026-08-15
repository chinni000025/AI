namespace AIEngineConnectivity.DTOs
{
    using AIEngineConnectivity.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class ScheduleEngineNotificationDTO
    {
        public Guid NotificationId { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTime RetryAt { get; set; }
    }
}
