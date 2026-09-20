namespace AIEngineConnectivity.Repositories
{
    public interface IEngineRecycleBinRepository
    {
        public Task DeleteRecycleItem(Guid id, CancellationToken cancellation);
    }
}