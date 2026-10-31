using AIEngineConnectivity.Services;

namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserRateLimiter : IUserRateLimiter
    {
        private readonly UserBucketStore _userBucketStore;

        public UserRateLimiter(UserBucketStore userBucketStore)
        {
            _userBucketStore = userBucketStore;
        }

        public bool TryAddRequest(string userId)
        {
            return _userBucketStore.TryAddRequest(userId);
        }
    }
}
