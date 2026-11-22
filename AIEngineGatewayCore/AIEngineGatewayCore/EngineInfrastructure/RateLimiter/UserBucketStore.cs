using AIEngineConnectivity.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserBucketStore
    {
        private readonly ConcurrentDictionary<string, UserBucket> _userBucketStore;
        private readonly int _initialCapacity;
        private readonly TimeSpan _leakInterval;
        private readonly TimeSpan _cleanUpInterval;

        public UserBucketStore(IOptions<UserRateLimiterOptions> options)
        {
            _initialCapacity = options.Value.InitialCapacity;
            _leakInterval = TimeSpan.FromSeconds(options.Value.LeakIntervalSeconds);
            _cleanUpInterval = TimeSpan.FromMinutes(options.Value.InActiveInterval);
            _userBucketStore = new ConcurrentDictionary<string, UserBucket>();
        }

        public UserBucket GetOrCreate(string userId)
        {
            return _userBucketStore.GetOrAdd(userId, _ => new UserBucket(_initialCapacity, _leakInterval, _cleanUpInterval));
        }

        public int CleanInActiveBuckets()
        {
            int removeCount = 0;
            foreach (var entry in _userBucketStore)
            {
                var UserId = entry.Key;
                var UserBucket = entry.Value;
                if (UserBucket.IsInActive())
                {
                    if (_userBucketStore.TryRemove(new KeyValuePair<string, UserBucket>(UserId, UserBucket)))
                    {
                        removeCount++;
                    }
                }
            }
            return removeCount;
        }

        public bool TryAddRequest(string userId)
        {
            var bucket = GetOrCreate(userId);
            return bucket.TryAddRequest();
        }
    }
}