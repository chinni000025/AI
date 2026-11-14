using AIEngineRedis.Abstraction;
using Microsoft.Extensions.Caching.Hybrid;

namespace AIEngineRedis.Services
{
    public class RedisCacheService(HybridCache hybridCache) : ICacheService
    {
        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
                TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration ?? TimeSpan.FromMinutes(10),
                LocalCacheExpiration = expiration ?? TimeSpan.FromMinutes(2)
            };
            return await hybridCache.GetOrCreateAsync(key, factory, options, cancellationToken: cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellation = default)
        {
            await hybridCache.RemoveAsync(key, cancellationToken: cancellation);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellation = default)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration ?? TimeSpan.FromMinutes(10),
                LocalCacheExpiration = expiration ?? TimeSpan.FromMinutes(2)
            };
            await hybridCache.SetAsync<T>(key, value, options, cancellationToken: cancellation);
        }
    }
}
