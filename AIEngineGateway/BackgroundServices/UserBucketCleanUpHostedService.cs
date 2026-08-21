using AIEngineGateway.EngineInfrastructure.UserRateLimiter;

namespace AIEngineGateway.BackgroundServices
{
    public class UserBucketCleanUpHostedService : BackgroundService
    {
        private readonly UserBucketStore _userBucketStore;
        private readonly ILogger<UserBucketCleanUpHostedService> _logger;
        public UserBucketCleanUpHostedService(UserBucketStore userBucket, ILogger<UserBucketCleanUpHostedService> logger)
        {
            _userBucketStore = userBucket;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    var removedCount = _userBucketStore.CleanInActiveBuckets();
                    if (removedCount > 0)
                    {
                        _logger.LogInformation($"User bucket cleanup completed. Removed {removedCount} inactive buckets.");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {

            }
        }
    }
}