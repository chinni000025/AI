namespace AIEngineRedis.Abstraction
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellation = default);
        public Task RemoveAsync(string key, CancellationToken cancellation = default);
    }
}