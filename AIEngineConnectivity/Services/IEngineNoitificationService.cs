namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineNoitificationService
    {
        public Task<Guid> AddEngineNotificationAsync(EngineNotification engineNotification, CancellationToken cancellationToken);
        public Task<EngineNotification?> GetEngineNotificationAsync(string engineNotificationId, CancellationToken cancellation);
        public Task RemoveEngineNotification(string engineNotificationId, CancellationToken cancellationToken);
    }
}
