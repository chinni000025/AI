using AIEngineConnectivity.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AIEngineRedis.Services
{
    public class RedisDistributedLockService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisDistributedLockService> logger)
        : IDistributedLockService
    {
        public async Task<IAsyncDisposable?> TryAcquireLockAsync(string resourceKey, TimeSpan expiry, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var db = connectionMultiplexer.GetDatabase();
            var lockKey = $"lock:{resourceKey}";
            var lockValue = Guid.NewGuid().ToString("N");
            var start = DateTime.UtcNow;
            while (!cancellationToken.IsCancellationRequested)
            {
                bool acquired = await db.LockTakeAsync(lockKey, lockValue, expiry);
                if (acquired)
                {
                    logger.LogDebug("Acquired distributed lock: {LockKey}", lockKey);
                    return new RedisLockHandle(db, lockKey, lockValue, logger);
                }

                if (timeout == TimeSpan.Zero || (DateTime.UtcNow - start) >= timeout)
                {
                    return null;
                }

                await Task.Delay(100, cancellationToken);
            }
            return null;
        }

        private sealed class RedisLockHandle : IAsyncDisposable
        {
            private readonly IDatabase _db;
            private readonly string _key;
            private readonly string _value;
            private readonly ILogger _logger;
            private int _disposed;
            public RedisLockHandle(IDatabase db, string key, string value, ILogger logger)
            {
                _db = db;
                _key = key;
                _value = value;
                _logger = logger;
            }
            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    bool released = await _db.LockReleaseAsync(_key, _value);
                    if (released)
                    {
                        _logger.LogDebug("Released distributed lock: {LockKey}", _key);
                    }
                    else
                    {
                        _logger.LogWarning("Distributed lock expired before release: {LockKey}", _key);
                    }
                }
            }
        }
    }
}
