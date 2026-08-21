namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserBucket
    {
        private readonly int _initialCapacity;
        private readonly TimeSpan _leakInterval;
        private int _bucket;
        private readonly Object _lock = new();

        private DateTime _nextLeakInterval;
        private DateTime _lastActivity;
        private TimeSpan _cleanUpInterval;
        public UserBucket(int initialCapacity, TimeSpan refillInterval, TimeSpan cleanUpInterval)
        {
            _initialCapacity = initialCapacity;
            _leakInterval = refillInterval;
            _bucket = 0;
            _nextLeakInterval = DateTime.UtcNow.Add(_leakInterval);
            _lastActivity = DateTime.UtcNow;
            _cleanUpInterval = cleanUpInterval;

        }

        public bool TryAddRequest()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                leakTokens(now);
                _lastActivity = now;
                if (_bucket < _initialCapacity)
                {
                    _bucket++;
                    return true;
                }
                return false;
            }
        }

        public bool IsInActive()
        {
            lock (_lock)
            {
                return DateTime.UtcNow - _lastActivity > _cleanUpInterval;
            }
        }

        private void leakTokens(DateTime now)
        {
            if (_bucket > 0 && now >= _nextLeakInterval)
            {
                _bucket--;
                _nextLeakInterval = _nextLeakInterval.Add(_leakInterval);
            }
            if (_bucket == 0)
            {
                _nextLeakInterval = now.Add(_leakInterval);
            }
        }
    }
}