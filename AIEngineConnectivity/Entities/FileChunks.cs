using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class FileChunks
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public long ChunkIndex { get; set; }
        public byte[]? Chunk { get; set; }

        public EngineFileUploadingSession EngineFileUploadingSession { get; set; }
    }
}
