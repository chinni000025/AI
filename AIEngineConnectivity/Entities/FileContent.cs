using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class FileContent
    {
        public Guid Id { get; set; }
        public uint? ContentOid { get; set; } // for Postgres
        public byte[]? ContentData { get; set; } // for SqlServer
        public EngineFile EngineFile { get; set; }
    }
}
