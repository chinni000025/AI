using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class FileContext
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public int? ConversationId { get; set; }
        public int? ChatId { get; set; }
        public EngineFile EngineFile { get; set; }
    }
}
