using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class FileContent
    {
        public Guid Id { get; set; }
        public byte[]? Content { get; set; }
    }
}
