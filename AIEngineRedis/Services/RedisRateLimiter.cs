using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AIEngineRedis.Services
{
    public class RedisRateLimiter : IUserRateLimiter, IServerRateLimiter
    {

        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRateLimiter> _logger;
        private readonly int _userCapacity;
        private readonly int _userWindowSeconds;
        private readonly int _serverCapacity;
        private readonly int _serverWindowSeconds;
        public RedisRateLimiter(
            IConnectionMultiplexer redis,
            IOptions<UserRateLimiterOptions> userOptions,
            IOptions<ServerRateLimiterOptions> serverOptions,
            ILogger<RedisRateLimiter> logger)
        {
            _redis = redis;
            _logger = logger;
            _userCapacity = userOptions.Value.InitialCapacity;
            _userWindowSeconds = Math.Max(1, userOptions.Value.LeakIntervalSeconds);
            _serverCapacity = serverOptions.Value.InitialCapacity;
            _serverWindowSeconds = Math.Max(1, serverOptions.Value.RefillIntervalSeconds);
        }
        public bool AllowRequest()
        {
            var db = _redis.GetDatabase();
            var key = "ratelimit:server:global";
            return CheckRateLimit(db, key, _serverCapacity, _serverWindowSeconds);
        }

        public bool TryAddRequest(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;
            var db = _redis.GetDatabase();
            var key = $"ratelimit:user:{userId}";
            return CheckRateLimit(db, key, _userCapacity, _userWindowSeconds);
        }

        private bool CheckRateLimit(IDatabase db, string key, int capacity, int windowSeconds)
        {
            try
            {
                long currentCount = db.StringIncrement(key);
                if (currentCount == 1)
                {
                    db.KeyExpire(key, TimeSpan.FromSeconds(windowSeconds));
                }
                return currentCount <= capacity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis rate limiting error on key {Key}. Allowing request as fallback.", key);
                return true;
            }
        }
    }
}
