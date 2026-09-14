using AIEngineConnectivity.Repositories;
using AIEngineGateway.PostMigrations;

namespace AIEngineGateway.BackgroundServices
{
    public class CleanSessionsAndChunks : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public CleanSessionsAndChunks(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanEngineUploadingSessionAndChunks(stoppingToken);
            }
        }

        private async Task CleanEngineUploadingSessionAndChunks(CancellationToken cancellationToken)
        {
           await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
            await repo.EngineDriveRepository.StaleEngineUploadingSessionsAndChunks(cancellationToken);
        }
    }
}
