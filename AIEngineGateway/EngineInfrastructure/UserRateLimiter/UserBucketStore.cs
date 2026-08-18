namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    using AIEngineConnectivity.Models;
    using Microsoft.Extensions.Options;
    using System.Collections.Concurrent;

    public sealed class UserBucketStore
    {
        private readonly ConcurrentDictionary<string, UserBucket> _userBucketStore;
        private readonly int _initialCapacity;
        private readonly TimeSpan _leakInterval;

        public UserBucketStore(IOptions<UserRateLimiterOptions> options)
        {
            _initialCapacity = options.Value.InitialCapacity;
            _leakInterval = TimeSpan.FromSeconds(options.Value.LeakIntervalSeconds);
            _userBucketStore = new ConcurrentDictionary<string, UserBucket>();
        }

        public UserBucket GetOrCreate(string userId)
        {
            return _userBucketStore.GetOrAdd(userId, _ => new UserBucket(_initialCapacity, _leakInterval));
        }

        public bool TryAddRequest(string userId)
        {
            var bucket = GetOrCreate(userId);
            return bucket.TryAddRequest();
        }
    }
}