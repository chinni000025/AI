using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Services
{
    public interface IEngineConfigService
    {
        bool IsEngineConfig();
        string ConnectionString();
        EngineConstants.DataBaseProvider GetDatabaseType();
        Task<bool> IsDataBaseExist(EngineConstants.DataBaseProvider dataBaseProvider, string connectionString);
    }
}