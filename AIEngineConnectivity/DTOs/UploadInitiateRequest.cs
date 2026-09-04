using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIEngineConnectivity.DTOs
{
    public class UploadInitiateRequest
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
        [JsonPropertyName("fileSize")]
        public long FileSize { get; set; }
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }
    }
}
