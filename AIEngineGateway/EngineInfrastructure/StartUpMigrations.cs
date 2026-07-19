
namespace AIEngineGateway.EngineInfrastructure
{
    using AIEngineGateway.Services;
    using Microsoft.EntityFrameworkCore;
    public class StartUpMigrations
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EngineConfig _engineConfig;
        private readonly ILogger<StartUpMigrations> _logger;
        public StartUpMigrations(IServiceProvider serviceProvider, EngineConfig engineConfig, ILogger<StartUpMigrations> logger)
        {
            _engineConfig = engineConfig;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ApplyMigrations()
        {
            try
            {
                if (!_engineConfig.IsEngineConfig())
                    return;

                using var scopedService = _serviceProvider.CreateScope();
                var EngineScheme = scopedService.ServiceProvider.GetRequiredService<EngineContext>();
                var pendingMigrations = await EngineScheme.Database.GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                    await EngineScheme.Database.MigrateAsync();

                var postMigrationsRunner = new PostMigrationRunner(EngineScheme);
                await postMigrationsRunner.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
