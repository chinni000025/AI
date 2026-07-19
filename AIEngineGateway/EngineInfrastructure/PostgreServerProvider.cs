namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Services;
    public class PostgreServerProvider : IDataBaseProvider
    {
        public string BuildConnectionString(DataBaseConfiguration dataBaseConfiguration)
        {
            return $"Host={dataBaseConfiguration.Server};" +
                   $"Port={dataBaseConfiguration.Port};" +
                   $"Username={dataBaseConfiguration.UserName};" +
                   $"Password={dataBaseConfiguration.Password};" +
                   $"SSL Mode=Prefer;" +
                   $"Trust Server Certificate=true;";
        }
    }
}
