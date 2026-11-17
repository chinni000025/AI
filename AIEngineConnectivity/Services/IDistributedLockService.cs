namespace AIEngineConnectivity.Services
{
    public interface IDistributedLockService
    {
        Task<IAsyncDisposable?> TryAcquireLockAsync(string resourceKey, TimeSpan expiry, TimeSpan timeout = default,
            CancellationToken cancellationToken = default);
    }
}
