using AIEngineConnectivity.Helpers;
using AIEngineConnectivity.Services;



namespace AIEngineGateway.Middlewares
{
#nullable disable
    public class ServerRateLimitingMiddleware
    {
        private ILogger<ServerRateLimitingMiddleware> _logger;
        private RequestDelegate _next;
        private IServerRateLimiter _serverRateLimiter;

        public ServerRateLimitingMiddleware(RequestDelegate next, ILogger<ServerRateLimitingMiddleware> logger,
            IServerRateLimiter serverRateLimiter)
        {
            _next = next;
            _logger = logger;
            _serverRateLimiter = serverRateLimiter;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<SkipLeakyBucketAttribute>() != null)
            {
                await _next(context);
                return;
            }
            var clientId = context.Connection.RemoteIpAddress.ToString();

            var allowed = _serverRateLimiter.AllowRequest();
            if (!allowed)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Too Many Request");
                _logger.LogError("Client Request To Many Requests");
                return;
            }
            await _next(context);
        }
    }
}
