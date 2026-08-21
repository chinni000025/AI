using AIEngineConnectivity.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Models
{
    public class GroupMessageAttachment
    {
        public int Id { get; set; }

        //Foreign key for the Message.
        public long GroupMessageId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        // File Size Bytes
        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        public GroupMessage GroupMessages { get; set; }
    }
}
