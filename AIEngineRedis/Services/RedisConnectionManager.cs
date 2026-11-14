using StackExchange.Redis;

namespace AIEngineRedis.Services
{
    public sealed class RedisConnectionManager : IDisposable
    {

        //HybridCache for application data caching(L1 + L2).

        //IConnectionMultiplexer for specialized Redis operations.

        //ICacheService as the application-facing abstraction.

        public IConnectionMultiplexer Connection { get; }
        public RedisConnectionManager(IConnectionMultiplexer connection)
        {
            Connection = connection;
        }

        public IDatabase GetDatabase(int database = -1)
        {
            return Connection.GetDatabase(database);
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
