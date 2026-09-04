using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;

namespace AIEngineGateway.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        public IIdentityRepository IdentityRepository { get; }

        private readonly EngineContext EngineContext;

        public IConversationRepository ConversationRepository { get; }
        public IConnectionRepository ConnectionRepository { get; }
        public IDataProtectionKeyRepository DataProtectionKeyRepository { get; }

        public IEngineNotificationRepository EngineNotificationRepository { get; }

        public IServiceProvider _ServiceProvider;
        public IEngineDriveRepository EngineDriveRepository { get; set; }

        public RepositoryWrapper(IIdentityRepository identityRepository, IConversationRepository conversationRepository,
            IConnectionRepository connectionRepository,
            IDataProtectionKeyRepository dataProtectionKeyRepository,
            IEngineNotificationRepository engineNotificationRepository,
            IEngineDriveRepository engineDriveRepository,
            EngineContext engineContext, IServiceProvider serviceProvider)
        {
            IdentityRepository = identityRepository;
            EngineContext = engineContext;
            ConversationRepository = conversationRepository;
            ConnectionRepository = connectionRepository;
            DataProtectionKeyRepository = dataProtectionKeyRepository;
            _ServiceProvider = serviceProvider;
            EngineNotificationRepository = engineNotificationRepository;
            EngineDriveRepository = engineDriveRepository;
        }

        public IEngineRepoBase<TEntity> GetEngineRepo<TEntity>() where TEntity : class
        {
            return _ServiceProvider.GetRequiredService<IEngineRepoBase<TEntity>>();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await EngineContext.SaveChangesAsync(cancellationToken); // 1 --> success.
        }
    }
}