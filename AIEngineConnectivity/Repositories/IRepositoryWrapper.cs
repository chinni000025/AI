using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IRepositoryWrapper
    {
        IIdentityRepository IdentityRepository { get; }
        IConversationRepository ConversationRepository { get; }
        IConnectionRepository ConnectionRepository { get; }
        IDataProtectionKeyRepository DataProtectionKeyRepository { get; }
        IEngineRepoBase<TEntity> GetEngineRepo<TEntity>() where TEntity : class;
        IEngineNotificationRepository EngineNotificationRepository { get; }
        IEngineDriveRepository EngineDriveRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
