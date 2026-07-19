namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    public class AIRequest
    {
        public string Prompt { get; set; }

        public string Model { get; set; }

        public string Provider { get; set; }

        public List<ChatContext> ConversationHistory { get; set; } = new List<ChatContext>();
    }

    public class ChatContext
    {
        public string Role { get; set; }

        public string Content { get; set; }
    }
}
