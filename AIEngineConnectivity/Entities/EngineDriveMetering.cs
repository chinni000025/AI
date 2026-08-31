using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AIEngineConnectivity.Entities
{
    public class EngineDriveMetering
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public long ReserveBytes { get; set; }
        public long ActiveBytes { get; set; }
        public long TrashBytes { get; set; }
        [NotMapped]
        public long TotalBytes => ReserveBytes + ActiveBytes + TrashBytes;
    }
}
