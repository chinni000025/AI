using AIEngineRedis.Abstraction;
using Microsoft.Extensions.Caching.Hybrid;

namespace AIEngineRedis.Services
{
    public class RedisCacheService(HybridCache hybridCache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await hybridCache.GetOrCreateAsync<T?>(key, _ => ValueTask.FromResult<T?>(default), cancellationToken: cancellationToken);
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
