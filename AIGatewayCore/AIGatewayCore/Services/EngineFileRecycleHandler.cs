using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class EngineFileRecycleHandler(IEngineDriveRepository engineDriveRepository)
        : IRecycleItemHandler
    {
        public async Task DeletePermanently(Guid id, CancellationToken cancellation)
        {
            await engineDriveRepository.DeleteEngineFileAsync(id, cancellation);
        }
    }
}