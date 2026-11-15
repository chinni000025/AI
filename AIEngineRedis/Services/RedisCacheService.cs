using AIEngineConnectivity.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace AIEngineRedis.Services
{
    public class RedisCacheService(HybridCache hybridCache) : ICacheService
    {
        public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
                TimeSpan? expiration = null, TimeSpan? localExpiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration ?? TimeSpan.FromMinutes(10),
                LocalCacheExpiration = localExpiration ?? TimeSpan.FromMinutes(2)
            };

            return await hybridCache.GetOrCreateAsync(key, factory, options, tags, cancellationToken: cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellation = default)
        {
            await hybridCache.RemoveAsync(key, cancellationToken: cancellation);
        }

        public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            await hybridCache.RemoveByTagAsync(tag, cancellationToken);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localExpiration = null, IEnumerable<string>? tags = null, CancellationToken cancellation = default)
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = expiration ?? TimeSpan.FromMinutes(10),
                LocalCacheExpiration = localExpiration ?? TimeSpan.FromMinutes(2)
            };
            await hybridCache.SetAsync<T>(key, value, options, tags, cancellationToken: cancellation);
        }
    }
}