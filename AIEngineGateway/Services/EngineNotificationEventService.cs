using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class EngineNotificationEventService : IEngineNotificationEventService
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


        public async Task<List<EngineNotificationEvent>> GetEventsByPriority(CancellationToken cancellationToken)
        {
            return await _repositoryWrapper.EngineNotificationRepository
                    .GetNotificationByPriority(cancellationToken);
        }

        public async void RemoveEngineNotificationEvent(Guid guid, CancellationToken cancellation)
        {
            var engineEventNotification = await _repositoryWrapper
                    .GetEngineRepo<EngineNotificationEvent>()
                    .GetByIdAsync(guid, cancellation);

            if (engineEventNotification is not null)
                _repositoryWrapper.GetEngineRepo<EngineNotificationEvent>().delete(engineEventNotification);

            await _repositoryWrapper.SaveChangesAsync(cancellation);
        }
    }
}
