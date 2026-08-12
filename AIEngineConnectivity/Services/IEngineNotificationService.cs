namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineNotificationService
    {
        public Task AddEngineNotificationAsync(EngineNotification engineNotification, CancellationToken cancellationToken);
        public Task<EngineNotification?> GetEngineNotificationAsync(Guid engineNotificationId, CancellationToken cancellation);
        public Task RemoveEngineNotification(Guid engineNotificationId, CancellationToken cancellationToken);
        public Task SaveChangesAsync(CancellationToken cancellation);
    }
}
