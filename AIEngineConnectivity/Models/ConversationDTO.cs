using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Models
{
#nullable disable
    public class ConversationDTO
    {
        public Guid ConversationId { get; set; }
        public string ConversationTitle { get; set; }
        public DateTime? LastMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ModelUsed { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsPinned { get; set; }
    }
}
