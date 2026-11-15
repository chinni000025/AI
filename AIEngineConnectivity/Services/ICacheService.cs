namespace AIEngineConnectivity.Services
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null,
                                TimeSpan? localExpiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localExpiration = null,
                            IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
    }
}