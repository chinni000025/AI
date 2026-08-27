using AIEngineConnectivity.Constants;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class EngineFile
    {
        public Guid Id { get; set; }
        public Guid ContentId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string? ParentId { get; set; }
        public string? Location { get; set; }
        public long FileSize { get; set; } = 0;
        public bool IsRecyled { get; set; } = false;
        public EngineFileType ItemType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedBy { get; set; }

        public FileContent FileContent { get; set; }
        public ICollection<FileContext> FileContexts { get; set; }

        public ICollection<FileAccessors> FileAccessors { get; set; }
    }
}
