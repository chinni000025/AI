namespace AIEngineConnectivity.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IRepositoryWrapper
    {
        IIdentityRepository IdentityRepository { get; }
        IConversationRepository ConversationRepository { get; }
        IConnectionRepository ConnectionRepository { get; }
        IDataProtectionKeyRepository DataProtectionKeyRepository { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
