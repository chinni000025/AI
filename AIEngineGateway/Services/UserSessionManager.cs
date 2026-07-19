namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Services;
    using System.Collections.Concurrent;
    public class UserSessionManager : IUserSessionManager
    {
        private readonly ConcurrentDictionary<string,
            ConcurrentDictionary<string, HashSet<string>>> _UserLoginDetails = new();
        public void AddConnection(string userId, string sessionId, string connectionId)
        {
            var sessions = _UserLoginDetails.GetOrAdd(userId, _ =>
                            new ConcurrentDictionary<string, HashSet<string>>());
            var connection = sessions.GetOrAdd(sessionId, _ => new HashSet<string>());
            lock (connection)
            {
                connection.Add(connectionId);
            }
        }

        public void RemoveConnection(string userId, string sessionId, string connectionId)
        {
            if (_UserLoginDetails.TryGetValue(userId, out var sessions))
            {
                if (sessions.TryGetValue(sessionId, out var connections))
                {
                    lock (connections)
                    {
                        connections.Remove(connectionId);
                        if (connections.Count == 0)
                        {
                            sessions.TryRemove(sessionId, out _);
                        }
                    }
                }
                if (sessions.IsEmpty)
                {
                    _UserLoginDetails.TryRemove(userId, out _);
                }
            }
        }

        public List<string> GetConnections(string userId, string sessionId)
        {
            if (_UserLoginDetails.TryGetValue(userId, out var sessions))
            {
                if (sessions.TryGetValue(sessionId, out var connections))
                {
                    return connections.ToList();
                }
            }
            return new List<string>();
        }

        public List<string> GetOtherSessionConnections(string userId, string currentSessionId)
        {
            var result = new List<string>();
            if (_UserLoginDetails.TryGetValue(userId, out var sessions))
            {
                foreach (var session in sessions)
                {
                    if (session.Key != currentSessionId)
                    {
                        result.AddRange(session.Value);
                    }
                }
            }
            return result;
        }

        public void RemoveSessions(string userId, string sessionId)
        {
            if (_UserLoginDetails.TryGetValue(userId, out var sessions))
            {
                sessions.TryRemove(sessionId, out _);
                if (sessions.IsEmpty)
                {
                    _UserLoginDetails.Remove(userId, out _);
                }
            }
        }
    }
}
