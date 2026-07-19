namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    public class ArchiveChatRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
    }
}