namespace AIEngineConnectivity.Services
{
    public interface IRecycleItemHandler
    {
        public Task DeletePermanently(Guid id, CancellationToken cancellation);
    }
}
