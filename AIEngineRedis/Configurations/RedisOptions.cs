namespace AIEngineRedis.Configurations
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";
        public string ConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = "AIEngine";
        public int Timeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public int ConnectRetry { get; set; } = 3;
        public bool AbortOnConnectFail { get; set; } = false;
        public bool Ssl { get; set; } = false;
    }
}
