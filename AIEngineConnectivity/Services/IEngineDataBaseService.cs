namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.DTOs;

    public interface IEngineDataBaseService
    {
        public Task ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration);
        public Task TestConnectionAsync(DataBaseConfiguration dataBaseConfiguration);
    }
}
