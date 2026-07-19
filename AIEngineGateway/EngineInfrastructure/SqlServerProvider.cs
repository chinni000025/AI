namespace AIEngineGateway.EngineInfrastructure
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Services;
    public class SqlServerProvider : IDataBaseProvider
    {
        public string BuildConnectionString(DataBaseConfiguration dataBaseConfiguration)
        {
            return $"Server = {dataBaseConfiguration.Server},{dataBaseConfiguration.Port};" +
                    $"User Id ={dataBaseConfiguration.UserName};" +
                    $"Password ={dataBaseConfiguration.Password};" +
                    $"Encrypt=True;" +
                    $"TrustServerCertificate=True;";
        }
    }
}
