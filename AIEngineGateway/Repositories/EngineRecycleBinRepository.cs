using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.Repositories
{
    public class EngineRecycleBinRepository(EngineContext engineContext) : IEngineRecycleBinRepository
    {
        public async Task DeleteRecycleItem(Guid id, CancellationToken cancellation)
        {
            await (from r in engineContext.RecycleBin
                   where r.Id == id
                   select r).ExecuteDeleteAsync(cancellation);
        }
    }
}