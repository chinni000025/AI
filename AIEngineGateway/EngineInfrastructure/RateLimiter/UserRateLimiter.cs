namespace AIEngineGateway.EngineInfrastructure.UserRateLimiter
{
    public sealed class UserRateLimiter
    {
        private readonly UserBucketStore _UserBucketStore;

        public UserRateLimiter(UserBucketStore userBucketStore)
        {
            _UserBucketStore = userBucketStore;
        }

        public bool TryAddRequest(string userId)
        {
            return _UserBucketStore.TryAddRequest(userId);
        }
    }
}