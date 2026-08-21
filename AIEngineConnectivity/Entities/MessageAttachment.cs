using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// This Model is Used for the Message Attachements.
    /// </summary>
    public class MessageAttachment
    {
        public int Id { get; set; }

        //Foreign key for the Message.
        public long MessageId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        // File Size Bytes
        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        public Message Messages { get; set; }
    }
}
