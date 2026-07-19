using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.DTOs
{
    public class ChatPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("model")]
        public string Model { get; init; }
        [System.Text.Json.Serialization.JsonPropertyName("messages")]
        public List<object> Messages { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;
    }
}
