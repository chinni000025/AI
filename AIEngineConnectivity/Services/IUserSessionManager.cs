namespace AIEngineConnectivity.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    public interface IUserSessionManager
    {
        public void AddConnection(string userId, string sessionId, string connectionId);

        public void RemoveConnection(string userId, string sessionId, string connectionId);

        public List<string> GetConnections(string userId, string sessionId);

        public List<string> GetOtherSessionConnections(string userId, string currentConnectionId);

        public void RemoveSessions(string userId, string sessionId);
    }
}
