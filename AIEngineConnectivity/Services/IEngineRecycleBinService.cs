using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Services
{
    public interface IEngineRecycleBinService
    {
        public Task DeleteRecycleItem(Guid Id, RecycleItem item, Guid entityId, CancellationToken cancellationToken);
    }
}
