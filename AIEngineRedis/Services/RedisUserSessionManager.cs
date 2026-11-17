using AIEngineConnectivity.Services;
using StackExchange.Redis;

namespace AIEngineRedis.Services
{
    public class RedisUserSessionManager : IUserSessionManager
    {

        private readonly IConnectionMultiplexer _redis;
        private static readonly TimeSpan SessionTtl = TimeSpan.FromDays(2);
        public RedisUserSessionManager(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        private IDatabase Db => _redis.GetDatabase();
        private static string UserSessionsKey(string userId) => $"user:{userId}:sessions";
        private static string SessionConnsKey(string userId, string sessionId) => $"user:{userId}:session:{sessionId}:conns";

        public void AddConnection(string userId, string sessionId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(connectionId))
                return;
            var db = Db;
            var sessionsKey = UserSessionsKey(userId);
            var connsKey = SessionConnsKey(userId, sessionId);

            db.SetAdd(sessionsKey, sessionId);
            db.KeyExpire(sessionsKey, SessionTtl);

            db.SetAdd(connsKey, connectionId);
            db.KeyExpire(connsKey, SessionTtl);
        }

        public List<string> GetConnections(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
                return new List<string>();

            var members = Db.SetMembers(SessionConnsKey(userId, sessionId));
            return members.Select(m => m.ToString()).ToList();
        }

        public List<string> GetOtherSessionConnections(string userId, string currentConnectionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<string>();
            var db = Db;
            var result = new List<string>();
            var allSessions = db.SetMembers(UserSessionsKey(userId));
            foreach (var session in allSessions)
            {
                var sessionStr = session.ToString();
                if (!string.Equals(sessionStr, currentConnectionId, StringComparison.OrdinalIgnoreCase))
                {
                    var conns = db.SetMembers(SessionConnsKey(userId, sessionStr));
                    result.AddRange(conns.Select(c => c.ToString()));
                }
            }
            return result;
        }

        public void RemoveConnection(string userId, string sessionId, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(connectionId))
                return;

            var db = Db;
            var connsKey = SessionConnsKey(userId, sessionId);

            db.SetRemove(connsKey, connectionId);
            if (db.SetLength(connsKey) == 0)
            {
                db.KeyDelete(connsKey);
                db.SetRemove(UserSessionsKey(userId), sessionId);
            }
        }

        public void RemoveSessions(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(sessionId))
                return;
            var db = Db;
            db.KeyDelete(SessionConnsKey(userId, sessionId));
            db.SetRemove(UserSessionsKey(userId), sessionId);
        }
    }
}
