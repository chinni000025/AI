namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// Used to store the Data Protection Keys
    /// </summary>
#nullable disable
    public class DataProtectionKey
    {
        public Guid Id { get; set; }
        public string ProtectionType { get; set; }
        public string ProtectionData { get; set; }
    }
}
