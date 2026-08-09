namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.EngineInfrastructure;
    using AIEngineGateway.Extensions;
    using Microsoft.EntityFrameworkCore;

    public class EngineConfigureService : IEngineDataBaseService
    {
        private readonly DataBaseIntialiationServices _dataBaseSetupServices;
        private readonly EngineConfig _engineConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly EngineState _engineState;
        public EngineConfigureService(DataBaseIntialiationServices dataBaseSetupServices,
            EngineConfig engineConfig, IServiceScopeFactory serviceScopeFactory, EngineState engineState)
        {
            _dataBaseSetupServices = dataBaseSetupServices;
            _engineConfig = engineConfig;
            _serviceScopeFactory = serviceScopeFactory;
            _engineState = engineState;
        }
        public async Task ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dataBaseProviderFactory = scope.ServiceProvider.GetRequiredService<IDataBaseProviderFactory>();
                var provider = dataBaseProviderFactory.GetDataBaseProvider(dataBaseConfiguration.DataBaseType)();
                var baseConnection = provider.BuildConnectionString(dataBaseConfiguration);

                await _dataBaseSetupServices.TestConnectionAsync(baseConnection, dataBaseConfiguration.DataBaseType);
                await _dataBaseSetupServices.EnsureDatabaseExistsAsync(baseConnection, dataBaseConfiguration.DatabaseName, dataBaseConfiguration.DataBaseType);

                var appConfiguration = baseConnection + $"Database={dataBaseConfiguration.DatabaseName}";
                using var context = dataBaseConfiguration.DataBaseType.GetEngineContextOptions(appConfiguration);

                await context.Database.MigrateAsync();
                _engineConfig.SaveEncryptedConnectionString(appConfiguration, dataBaseConfiguration.DataBaseType);
                _engineState.IsEngineReady = false;
                _engineState.ErrorMessage = null;

                _ = Task.Run(async () =>
                {
                    var engineStartupService = scope.ServiceProvider.GetRequiredService<IEngineStartUpService>();
                    await engineStartupService.InitializeAsync();
                });
            }
            catch
            {
                throw;
            }
        }

        public async Task TestConnectionAsync(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dataBaseProviderFactory = scope.ServiceProvider.GetRequiredService<IDataBaseProviderFactory>();
                var provider = dataBaseProviderFactory.GetDataBaseProvider(dataBaseConfiguration.DataBaseType)();
                var baseConnection = provider.BuildConnectionString(dataBaseConfiguration);

                await _dataBaseSetupServices.TestConnectionAsync(baseConnection, dataBaseConfiguration.DataBaseType);
            }
            catch
            {
                throw;
            }
        }
    }
}
