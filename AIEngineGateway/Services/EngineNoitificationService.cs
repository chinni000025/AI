namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Repositories;

    public class EngineNoitificationService : IEngineNoitificationService
    {
        private readonly IRepositoryWrapper _Repository;

        public EngineNoitificationService(IRepositoryWrapper repository)
        {
            _Repository = repository;
        }

        public async Task<Guid> AddEngineNotificationAsync(EngineNotification engineNotification, CancellationToken cancellationToken)
        {
            await _Repository.GetEngineRepo<EngineNotification>().AddAsync(engineNotification, cancellationToken);
            await _Repository.SaveChangesAsync(cancellationToken);
            return engineNotification.Id;
        }

        public async Task<EngineNotification?> GetEngineNotificationAsync(string engineNotificationId, CancellationToken cancellation)
        {
            return await _Repository.GetEngineRepo<EngineNotification>()
                           .GetByIdAsync(engineNotificationId, cancellation);
        }

        public async Task RemoveEngineNotification(string engineNotificationId, CancellationToken cancellationToken)
        {
            var engineNotification = await GetEngineNotificationAsync(engineNotificationId, cancellationToken);
            if (engineNotification is not null)
            {
                _Repository.GetEngineRepo<EngineNotification>().delete(engineNotification);
                await _Repository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}