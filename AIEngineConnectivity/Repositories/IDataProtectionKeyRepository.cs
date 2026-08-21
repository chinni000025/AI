using AIEngineConnectivity.Entities;

namespace AIEngineConnectivity.Repositories
{
    public interface IDataProtectionKeyRepository
    {
        Task<DataProtectionKey?> GetKeyAsync(string name, CancellationToken cancellationToken);
    }
}
