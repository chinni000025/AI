namespace AIEngineGateway.Repositories
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.EntityFrameworkCore;

    public class DataProtectionKeyRepository : IDataProtectionKeyRepository
    {
        private readonly EngineContext _EngineContext;
        private readonly EngineConfig _EngineConfig;

        public DataProtectionKeyRepository(EngineContext engineContext, EngineConfig engineConfig)
        {
            _EngineContext = engineContext;
            _EngineConfig = engineConfig;
        }

        public async Task<DataProtectionKey?> GetKeyAsync(string name, CancellationToken cancellationToken)
        {
            if (_EngineConfig.IsEngineConfig())
            {
                var dataProtectionKey = await (from d in _EngineContext.DataProtectionKeys
                                               where d.ProtectionType.Equals(name)
                                               select d).AsNoTracking()
                                               .FirstOrDefaultAsync(cancellationToken);
                return dataProtectionKey;
            }
            return null;
        }
    }
}
