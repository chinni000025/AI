using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.DTOs
{
    public class ConversationWithMessages
    {
        public Guid ConversationId { get; set; }
        public string Title { get; set; }
        public string ModelUsed { get; set; }
        public List<ConversationMessages> Messages { get; set; }
    }
    public class ConversationMessages
    {
        public long MessageId { get; set; }
        public string Content { get; set; }
        public int RoleId { get; set; }
        public DateTime MessagSentAt { get; set; }
    }
}
