namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ConversationPathDTO
    {
        public bool? IsPinned { get; set; }
        public bool? IsFavorite { get; set; }
        public string? Title { get; set; }
        public bool? IsArchived { get; set; }
        public string? ModelUsed { get; set; }
    }
}
