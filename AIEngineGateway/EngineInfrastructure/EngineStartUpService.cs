namespace AIEngineGateway.EngineInfrastructure
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Hub;
    using Microsoft.AspNetCore.SignalR;

    public class EngineStartUpService : IEngineStartUpService
    {
        private StartUpMigrations _startUpMigrations;
        private IServiceProvider _serviceProvider;
        private EngineState _engineState;
        private ILogger<EngineStartUpService> _logger;
        private IHubContext<EngineStatusHub> _hubContext;
        public EngineStartUpService(StartUpMigrations startUpMigrations, IServiceProvider serviceProvider,
            EngineState engineState, ILogger<EngineStartUpService> logger, IHubContext<EngineStatusHub> hubContext)
        {
            _startUpMigrations = startUpMigrations;
            _serviceProvider = serviceProvider;
            _engineState = engineState;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task InitializeAsync()
        {
            if (_engineState.IsEngineReady || _engineState.IsEngineRunning)
                return;

            _engineState.IsEngineReady = false;
            _engineState.IsEngineRunning = true;
            _engineState.ErrorMessage = null;

            try
            {
                await _startUpMigrations.ApplyMigrations();
                using var scope = _serviceProvider.CreateAsyncScope();
                var whisperService = scope.ServiceProvider.GetRequiredService<IWhisperService>();
                await whisperService.InitializeAsync();
                _engineState.IsEngineReady = true;
                await _hubContext.Clients.All.SendAsync(EngineConstants.EngineStateChanged, new
                {

                    isEngineRunning = _engineState.IsEngineRunning,
                    isEngineReady = _engineState.IsEngineReady,
                    errorMessage = _engineState.ErrorMessage
                });

            }
            catch (Exception ex)
            {
                _engineState.IsEngineReady = false;
                _engineState.ErrorMessage = $"Engine setup failed. Please check your internet connectivity.{ex.Message}";
                _logger.LogError($"Error Occured On EngineStartup Service  {ex.Message}");
                throw new Exception(ex.Message);
            }
            finally
            {
                _engineState.IsEngineRunning = false;
            }
        }
    }
}
