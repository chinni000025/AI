
namespace AIEngineGateway.Repositories
{
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.EngineInfrastructure;

    public class RepositoryWrapper : IRepositoryWrapper
    {
        public IIdentityRepository IdentityRepository { get; }

        private readonly EngineContext EngineContext;

        public IConversationRepository ConversationRepository { get; }
        public IConnectionRepository ConnectionRepository { get; }
        public IDataProtectionKeyRepository DataProtectionKeyRepository { get; }

        public RepositoryWrapper(IIdentityRepository identityRepository, IConversationRepository conversationRepository,
            IConnectionRepository connectionRepository,
            IDataProtectionKeyRepository dataProtectionKeyRepository,
            EngineContext engineContext)
        {
            IdentityRepository = identityRepository;
            EngineContext = engineContext;
            ConversationRepository = conversationRepository;
            ConnectionRepository = connectionRepository;
            DataProtectionKeyRepository = dataProtectionKeyRepository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await EngineContext.SaveChangesAsync(cancellationToken); // 1 --> success.
        }
    }
}
