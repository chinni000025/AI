namespace AIEngineConnectivity.Models
{
#nullable disable
    /// <summary>
    /// Used for add the message to convesation
    /// </summary>
    public class MessagePayload
    {
        public string Content { get; set; }
        public string Model { get; set; }
        public string Provider { get; set; }
    }
}
