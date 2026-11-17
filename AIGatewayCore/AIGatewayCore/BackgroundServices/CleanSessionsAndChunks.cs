using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEngineGateway.BackgroundServices
{
    public class CleanSessionsAndChunks : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDistributedLockService _distributedLockService;
        private readonly ILogger<CleanSessionsAndChunks> _logger;
        public CleanSessionsAndChunks(IServiceScopeFactory serviceScopeFactory, IDistributedLockService lockService,
            ILogger<CleanSessionsAndChunks> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _distributedLockService = lockService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await using var lockHandle = await _distributedLockService.TryAcquireLockAsync("cleanup:sessions-and-chunks",
                    expiry: TimeSpan.FromMinutes(5), timeout: TimeSpan.Zero, stoppingToken);

                if (lockHandle is null)
                {
                    _logger.LogInformation("Another instance is already cleaning sessions/chunks. Skipping this cycle.");
                    continue;
                }
                await CleanEngineUploadingSessionAndChunks(stoppingToken);
            }
        }

        private async Task CleanEngineUploadingSessionAndChunks(CancellationToken cancellationToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
            await repo.ExecuteInTransactionAsync(async () =>
             {
                 await repo.EngineDriveRepository.StaleEngineUploadingSessionsAndChunks(cancellationToken);
             }, cancellationToken);
        }
    }
}
