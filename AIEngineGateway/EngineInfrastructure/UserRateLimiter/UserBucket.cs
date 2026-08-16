namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserBucket
    {
        private readonly int _initialCapacity;
        private readonly TimeSpan _refillInterval;
        private readonly Queue<Token> _bucket;
        private Object _lock = new Object();

        public UserBucket(int initialCapacity, TimeSpan refillInterval)
        {
            _initialCapacity = initialCapacity;
            _refillInterval = refillInterval;
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
            Thread leakThread = new Thread(() =>
            {
                while (true)
                {
                    lock (_lock)
                    {
                        if (_bucket.Count > 0)
                        {
                            _bucket.Dequeue();
                        }
                    }
                    Thread.Sleep(_refillInterval * 1000);
                }
            });
            leakThread.IsBackground = true;
            leakThread.Start();
        }
    }

    public sealed record Token(DateTime CreatedAt);
}