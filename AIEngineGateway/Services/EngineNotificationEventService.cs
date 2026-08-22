using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Services
{
    public class EngineNotificationEventService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public EngineNotificationEventService(IRepositoryWrapper engineContext)
        {
            _repositoryWrapper = engineContext;
        }

        public async Task InsertEventNotification(EngineNotificationEvent engineNotificationEvent, CancellationToken cancellationToken)
        {
            await _repositoryWrapper.GetEngineRepo<EngineNotificationEvent>()
                    .AddAsync(engineNotificationEvent, cancellationToken);
            await _repositoryWrapper.SaveChangesAsync(cancellationToken);
        }


    }
}
