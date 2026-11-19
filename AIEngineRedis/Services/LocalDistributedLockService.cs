using AIEngineConnectivity.Services;
using System.Collections.Concurrent;

namespace AIEngineRedis.Services
{
    public class LocalDistributedLockService : IDistributedLockService
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        public async Task<IAsyncDisposable?> TryAcquireLockAsync(string resourceKey, TimeSpan expiry, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {

            var semaphoreSlim = _locks.GetOrAdd(resourceKey, _ => new SemaphoreSlim(1, 1));

            bool entered = await semaphoreSlim.WaitAsync(timeout, cancellationToken);
            if (!entered)
            {
                return null;
            }

            return new LocalLockHandle(semaphoreSlim);
        }
        private sealed class LocalLockHandle(SemaphoreSlim semaphoreSlim) : IAsyncDisposable
        {
            private int _dispose;
            public ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _dispose, 1) == 0)
                {
                    semaphoreSlim.Release();
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
