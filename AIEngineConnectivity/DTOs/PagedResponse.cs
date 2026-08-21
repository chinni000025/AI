using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.DTOs
{
#nullable disable
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}