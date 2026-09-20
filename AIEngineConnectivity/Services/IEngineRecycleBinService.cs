using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;

namespace AIEngineConnectivity.Services
{
    public interface IEngineRecycleBinService
    {
        public Task moveToRecycleAsync(RecycleBin entity, CancellationToken cancellationToken);
        public Task moveToRecycleAsync(IEnumerable<RecycleBin> entities, CancellationToken cancellationToken);
        public Task DeleteRecycleItem(Guid Id, RecycleItem item, Guid entityId, CancellationToken cancellationToken);
    }
}
