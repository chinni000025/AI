using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Services;
using AIEngineGateway.EngineInfrastructure;
using AIEngineGateway.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AIEngineGateway.Services
{
    public class EngineConfigureService : IEngineDataBaseService
    {
        private readonly DataBaseIntialiationServices _dataBaseSetupServices;
        private readonly EngineConfig _engineConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly EngineState _engineState;
        private readonly EngineRecoveryKeyService _engineRecoveryKeyService;

        public EngineConfigureService(DataBaseIntialiationServices dataBaseSetupServices,
            EngineConfig engineConfig, IServiceScopeFactory serviceScopeFactory, EngineState engineState, EngineRecoveryKeyService engineRecoveryKeyService)
        {
            _dataBaseSetupServices = dataBaseSetupServices;
            _engineConfig = engineConfig;
            _serviceScopeFactory = serviceScopeFactory;
            _engineState = engineState;
            _engineRecoveryKeyService = engineRecoveryKeyService;
        }

        public async Task ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration)
        {
            if (string.IsNullOrWhiteSpace(dataBaseConfiguration.RecoveryKey))
                throw new InvalidOperationException("Engine Key is required.");

            if (!_engineRecoveryKeyService.TryParse(dataBaseConfiguration.RecoveryKey, out var rawKey))
                throw new InvalidOperationException("Invalid Engine Key format. Please enter a valid key or click Generate Key.");

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dataBaseProviderFactory = scope.ServiceProvider.GetRequiredService<IDataBaseProviderFactory>();
                var provider = dataBaseProviderFactory.GetDataBaseProvider(dataBaseConfiguration.DataBaseType)();
                var baseConnection = provider.BuildConnectionString(dataBaseConfiguration);

                await _dataBaseSetupServices.TestConnectionAsync(baseConnection, dataBaseConfiguration.DataBaseType);
                bool isDataBaseExists = await _dataBaseSetupServices.DataBaseExistsAsync(baseConnection,
                    dataBaseConfiguration.DatabaseName, dataBaseConfiguration.DataBaseType);


                var appConfiguration = baseConnection + $"Database={dataBaseConfiguration.DatabaseName};";
                using var context = dataBaseConfiguration.DataBaseType.GetEngineContextOptions(appConfiguration);
                if (!isDataBaseExists)
                {
                    await _dataBaseSetupServices.EnsureDatabaseExistsAsync(baseConnection, dataBaseConfiguration.DatabaseName, dataBaseConfiguration.DataBaseType);
                    await context.Database.MigrateAsync();
                    var id = Guid.NewGuid();
                    var verifier = _engineRecoveryKeyService.ComputeVerifier(rawKey, id);

                    await context.Set<EngineIdentity>().AddAsync(new EngineIdentity
                    {
                        Id = id,
                        ConfigureOn = DateTime.UtcNow,
                        KeyVerifier = verifier
                    }); // Seeding the Verifier.

                    await context.SaveChangesAsync();
                }
                else
                {
                    var existingIdentity = await TryGetIdentityAsync(context);
                    if (existingIdentity is not null)
                    {
                        bool isKeyValid = _engineRecoveryKeyService.Matches(existingIdentity.KeyVerifier, rawKey, existingIdentity.Id);
                        if (!isKeyValid)
                        {
                            throw new InvalidOperationException("Access Denied: The provided Engine Key does not match this existing database.");
                        }
                        await context.Database.MigrateAsync();
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }

                _engineConfig.SaveEncryptedConnectionString(appConfiguration, dataBaseConfiguration.DataBaseType);
                _engineState.IsEngineReady = false;
                _engineState.ErrorMessage = null;

                _ = Task.Run(async () =>
                {
                    await using var bgScope = _serviceScopeFactory.CreateAsyncScope();
                    var engineStartupService = bgScope.ServiceProvider.GetRequiredService<IEngineStartUpService>();
                    await engineStartupService.InitializeAsync();
                });
            }
            catch
            {
                throw;
            }
            finally
            {
                if (rawKey != null)
                {
                    CryptographicOperations.ZeroMemory(rawKey);
                }
            }
        }

        private async Task<EngineIdentity?> TryGetIdentityAsync(EngineContext context)
        {
            return await context.Set<EngineIdentity>().AsNoTracking().SingleOrDefaultAsync();
        }

        public async Task TestConnectionAsync(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dataBaseConfiguration.RecoveryKey))
                    throw new Exception("Engine Key is required.");

                if (!_engineRecoveryKeyService.TryParse(dataBaseConfiguration.RecoveryKey, out var rawKey))
                {
                    throw new InvalidOperationException("Invalid Engine Key format. Please enter a valid key or click Generate Key.");
                }
                using var scope = _serviceScopeFactory.CreateScope();
                var dataBaseProviderFactory = scope.ServiceProvider.GetRequiredService<IDataBaseProviderFactory>();
                var provider = dataBaseProviderFactory.GetDataBaseProvider(dataBaseConfiguration.DataBaseType)();
                var baseConnection = provider.BuildConnectionString(dataBaseConfiguration);

                await _dataBaseSetupServices.TestConnectionAsync(baseConnection, dataBaseConfiguration.DataBaseType);
                bool isDataBaseExists = await _dataBaseSetupServices.DataBaseExistsAsync(baseConnection, dataBaseConfiguration.DatabaseName,
                               dataBaseConfiguration.DataBaseType);
                if (!isDataBaseExists)
                    return; // New DataBase.

                var appConfiguration = baseConnection + $"Database={dataBaseConfiguration.DatabaseName}";
                using var context = dataBaseConfiguration.DataBaseType.GetEngineContextOptions(appConfiguration);

                var existingIdentity = await TryGetIdentityAsync(context);
                if (existingIdentity is not null)
                {
                    bool isKeyValid = _engineRecoveryKeyService.Matches(existingIdentity.KeyVerifier, rawKey, existingIdentity.Id);
                    if (!isKeyValid)
                    {
                        throw new InvalidOperationException("Access Denied: The provided Engine Key does not match this existing database.");
                    }
                    return;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
