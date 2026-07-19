namespace AIEngineConnectivity.Repositories
{
    using AIEngineConnectivity.Entities;
    public interface IDataProtectionKeyRepository
    {
        Task<DataProtectionKey?> GetKeyAsync(string name, CancellationToken cancellationToken);
    }
}
