namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Repositories;

    public class EngineNotificationService : IEngineNotificationService
    {
        private readonly IRepositoryWrapper _Repository;

        public EngineNotificationService(IRepositoryWrapper repository)
        {
            _Repository = repository;
        }

        public async Task AddEngineNotificationAsync(EngineNotification engineNotification, CancellationToken cancellationToken)
        {
            await _Repository.GetEngineRepo<AIEngineConnectivity.Entities.EngineNotification>().AddAsync(engineNotification, cancellationToken);
        }

        public async Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation)
        {
            return await _Repository.GetEngineRepo<AIEngineConnectivity.Entities.EngineNotification>()
                           .GetByIdAsync(engineNotificationId, cancellation);
        }

        public async Task RemoveEngineNotification(Guid engineNotificationId, CancellationToken cancellationToken)
        {
            var engineNotification = await GetEngineNotificationAsync(engineNotificationId, cancellationToken);
            if (engineNotification is not null)
            {
                _Repository.GetEngineRepo<AIEngineConnectivity.Entities.EngineNotification>().delete(engineNotification);
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellation)
        {
            await _Repository.SaveChangesAsync(cancellation);
        }
    }
}