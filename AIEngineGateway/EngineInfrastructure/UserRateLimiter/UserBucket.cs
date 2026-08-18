namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserBucket
    {
        private readonly int _initialCapacity;
        private readonly TimeSpan _leakInterval;
        private readonly Queue<Token> _bucket;
        private Object _lock = new Object();

        private DateTime _nextLeakInterval;

        public UserBucket(int initialCapacity, TimeSpan refillInterval)
        {
            _initialCapacity = initialCapacity;
            _leakInterval = refillInterval;
            _bucket = new Queue<Token>();
        }

        public bool TryAddRequest()
        {
            lock (_lock)
            {
                if (_bucket.Count < _initialCapacity)
                {
                    _bucket.Enqueue(new Token(DateTime.Now));
                    return true;
                }
                return false;
            }
        }

        public void leakTokens()
        {

        }
    }
}

public sealed record Token(DateTime CreatedAt);
}