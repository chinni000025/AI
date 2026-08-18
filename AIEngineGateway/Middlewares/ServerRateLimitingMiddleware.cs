namespace AIEngineGateway.Middlewares
{
    using AIEngineGateway.Services;
#nullable disable
    public class ServerRateLimitingMiddleware
    {
        private ILogger<ServerRateLimitingMiddleware> _logger;
        private RequestDelegate _next;

        public ServerRateLimitingMiddleware(RequestDelegate next, ILogger<ServerRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = context.Connection.RemoteIpAddress.ToString();

            var allowed = LeakyBucket.EnqueueRequest(clientId);
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
