using AIEngineConnectivity.DTOs;

namespace AIEngineConnectivity.Services
{
    public interface IEngineDataBaseService
    {
        public Task ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration);
        public Task TestConnectionAsync(DataBaseConfiguration dataBaseConfiguration);
    }
}
