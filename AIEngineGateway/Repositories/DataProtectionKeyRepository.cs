namespace AIEngineGateway.Repositories
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.EntityFrameworkCore;

    public class DataProtectionKeyRepository : IDataProtectionKeyRepository
    {
        private readonly EngineContext _EngineContext;

        public DataProtectionKeyRepository(EngineContext engineContext)
        {
            _EngineContext = engineContext;
        }

        public async Task<DataProtectionKey?> GetKeyAsync(string name, CancellationToken cancellationToken)
        {
            var dataProtectionKey = await (from d in _EngineContext.DataProtectionKeys
                                           where d.ProtectionType.Equals(name)
                                           select d).AsNoTracking()
                                           .FirstOrDefaultAsync(cancellationToken);
            return dataProtectionKey;
        }
    }
}
