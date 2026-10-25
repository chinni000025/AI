namespace AIEngineConnectivity.Entities
{
    public class EngineIdentity
    {
        public Guid Id { get; set; }
        public DateTime ConfigureOn { get; set; }
        public string KeyVerifier { get; set; }
    }
}
