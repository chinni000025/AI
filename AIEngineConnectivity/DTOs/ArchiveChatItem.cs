namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ArchiveChatItem
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; }
        public string Preview { get; set; }
        public int MessageCount { get; set; }
        public DateTime ArchivedAt { get; set; }
    }
}