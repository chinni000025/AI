namespace AIEngineConnectivity.Entities
{
    public class EngineConnection
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionInfo { get; set; }
        public bool IsConnected { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
