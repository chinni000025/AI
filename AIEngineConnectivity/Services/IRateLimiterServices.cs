namespace AIEngineConnectivity.Services
{
    public interface IServerRateLimiter
    {
        bool AllowRequest();
    }

    public interface IUserRateLimiter
    {
        bool TryAddRequest(string userId);
    }
}
