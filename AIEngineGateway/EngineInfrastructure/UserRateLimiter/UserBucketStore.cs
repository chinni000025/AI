namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    using System.Collections.Concurrent;

    public sealed class UserBucketStore
    {
        private readonly ConcurrentDictionary<string, UserBucket> _userBucketStore;
        private readonly int _initialCapacity;
        private readonly TimeSpan _refillInterval;
        public UserBucketStore(int initialCapacity, TimeSpan refillInterval)
        {
            _initialCapacity = initialCapacity;
            _refillInterval = refillInterval;
            _userBucketStore = new ConcurrentDictionary<string, UserBucket>();
        }

        public UserBucket GetOrCreate(string userId)
        {
            return _userBucketStore.GetOrAdd(userId, _ => new UserBucket(_initialCapacity, _refillInterval));
        }

        public bool TryAddRequest(string userId)
        {
            var bucket = GetOrCreate(userId);
            return bucket.TryAddRequest();
        }
    }
}