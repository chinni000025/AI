using AIEngineGateway.EngineInfrastructure.DatabaseScripts;
using Microsoft.EntityFrameworkCore;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.EngineInfrastructure
{
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
                await EnsureQuartzTablesExistAsync(EngineScheme, _engineConfig.GetDatabaseType());
                var postMigrationsRunner = new PostMigrationRunner(EngineScheme);
                await postMigrationsRunner.RunAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task EnsureQuartzTablesExistAsync(EngineContext context, DataBaseProvider dbProvider)
        {
            var scriptFile = dbProvider switch
            {
                DataBaseProvider.SqlServer => "Quartz_SqlServer.sql",
                DataBaseProvider.PostgreSql => "Quartz_Postgres.sql",
                _ => throw new NotSupportedException($"Unsupported database provider: {dbProvider}")
            };
            var sqlScripts = await ScriptLoader.LoadScriptsAsync(scriptFile);
            await context.Database.ExecuteSqlRawAsync(sqlScripts);
        }
    }
}
