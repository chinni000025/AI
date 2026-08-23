using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.BackgroundServices
{
    public class RestoreEngineNotifications : BackgroundService
    {
        private readonly ILogger<RestoreEngineNotifications> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RestoreEngineNotifications(ILogger<RestoreEngineNotifications> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await using var scope = _serviceScopeFactory.CreateAsyncScope();
                    var repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
                    var engineLatch = scope.ServiceProvider.GetRequiredService<IEngineLatch>();
                    var engineQueue = scope.ServiceProvider.GetRequiredService<IEngineQueue<EngineNotificationMessage>>();
                    var engineNotificationService = scope.ServiceProvider.GetRequiredService<IEngineNotificationService>();
                    foreach (var engineEvents in await repositoryWrapper
                        .EngineNotificationRepository.GetNotificationByPriority(stoppingToken))
                    {
                        var engineNotificationMessage = engineLatch.Deserialize<EngineNotificationMessage>(engineEvents.EventData);
                        if (await engineNotificationService.IsValidNotification(engineNotificationMessage.NotificationId!.Value, stoppingToken))
                        {
                            await engineQueue.publishAsync(engineNotificationMessage, engineNotificationMessage.NotificationPriority, stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured on Restore Engine Notitification : " + ex);
            }
        }
    }
}
