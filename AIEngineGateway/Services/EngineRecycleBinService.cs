using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class EngineRecycleBinService(IRepositoryWrapper repository,
        IRecycleItemHandler recycleItemHandler, IServiceProvider serviceProvider,
        IEngineScheduler engineScheduler) : IEngineRecycleBinService
    {
        public async Task moveToRecycleAsync(RecycleBin entity,
            CancellationToken cancellationToken)
        {
            await engineScheduler.DeleteEngineRecycleItems(entity, cancellationToken);
            await repository.GetEngineRepo<RecycleBin>().AddAsync(entity, cancellationToken);
        }

        public async Task moveToRecycleAsync(IEnumerable<RecycleBin> entities,
            CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                await engineScheduler.DeleteEngineRecycleItems(entity, cancellationToken);
            }
            await repository.GetEngineRepo<RecycleBin>().AddRangeAsync(entities, cancellationToken);
        }

        public async Task DeleteRecycleItem(Guid Id, RecycleItem item, Guid entityId, CancellationToken cancellationToken)
        {
            await repository.ExecuteInTransactionAsync(async () =>
            {
                var service = serviceProvider
                .GetKeyedService<IRecycleItemHandler>(item) ??
                throw new Exception($"No Handler are registered for the Key {item}");

                await service.DeletePermanently(entityId, cancellationToken);
                await repository.EngineRecycleBinRepository
                    .DeleteRecycleItem(Id, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }
    }
}