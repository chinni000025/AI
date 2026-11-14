namespace AIEngineRedis.Abstraction
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellation = default);
        public Task RemoveAsync(string key, CancellationToken cancellation = default);
    }
}