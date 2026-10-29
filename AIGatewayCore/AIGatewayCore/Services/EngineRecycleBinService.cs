using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class EngineRecycleBinService(IRepositoryWrapper repository, IServiceProvider serviceProvider) : IEngineRecycleBinService
    {
        public async Task DeleteRecycleItem(Guid Id, RecycleItem item, Guid entityId, CancellationToken cancellationToken)
        {
            await repository.ExecuteInTransactionAsync(async () =>
            {
                var service = serviceProvider.GetKeyedService<IRecycleItemHandler>(item) ??
                throw new Exception($"No Handler are registered for the Key {item}");

                await service.DeletePermanently(entityId, cancellationToken);
                await repository.EngineRecycleBinRepository
                    .DeleteRecycleItem(Id, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }
    }
}