namespace AIEngineConnectivity.Repositories
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IConnectionRepository
    {
        public Task<EngineConnection?> GetConnectionsByUserId(String userId, String connectionType, CancellationToken cancellationToken);
    }
}
