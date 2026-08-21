using AIEngineConnectivity.Models;
using Microsoft.Extensions.Options;

namespace AIEngineGateway.EngineInfrastructure.RateLimiter
{
    public class ServerRateLimiter
    {
        private int _serverLimiter;
        private readonly int _initialCount;
        private readonly TimeSpan _refillInterval;
        private readonly object _lock = new();
        private DateTime _lastRequestedAt;
        public ServerRateLimiter(IOptions<ServerRateLimiterOptions> options)
        {
            _initialCount = options.Value.InitialCapacity;
            _refillInterval = TimeSpan.FromSeconds(options.Value.RefillIntervalSeconds);
            _serverLimiter = _initialCount;
            _lastRequestedAt = DateTime.UtcNow;
        }

        public bool AllowRequest()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                leakRequests(now);
                if (_serverLimiter == 0)
                {
                    return false;
                }
                _lastRequestedAt = now;
                _serverLimiter--;
                return true;
            }
        }

        private void leakRequests(DateTime now)
        {
            while (now - _lastRequestedAt >= _refillInterval && _serverLimiter < _initialCount)
            {
                _serverLimiter++;
                _lastRequestedAt = _lastRequestedAt.Add(_refillInterval);
            }
        }
    }
}
