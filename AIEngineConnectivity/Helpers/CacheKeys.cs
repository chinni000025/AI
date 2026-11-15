namespace AIEngineConnectivity.Helpers
{
    public static class CacheKeys
    {
        public static string Conversation(Guid id) => $"conv:{id}";
        public static string UserConversations(string userId) => $"user:{userId}:convs";
        public static string UserProjects(string userId) => $"user:{userId}:projects";
        public static string UserProfile(string userId) => $"user:{userId}:profile";
        public static string RefreshToken(string tokenHash) => $"token:refresh:{tokenHash}";

        // Tags for bulk eviction
        public static string TagUser(string userId) => $"tag:user:{userId}";
        public static string TagConversation(Guid id) => $"tag:conv:{id}";
    }

    public static class CachePolicies
    {
        public const string UserCachePolicy = "UserCache";
        public const string GlobalCachePolicy = "GlobalCache";
        public const string ShortLivedCachePolicy = "ShortLivedCache";
    }

    public static class PoliciesTags
    {
        public const string UserDataTag = "user-data";
        public const string GlobalDataTag = "global-data";
        public const string StatusDataTag = "status-data";
    }
}