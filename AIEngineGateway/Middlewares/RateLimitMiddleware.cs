namespace AIEngineGateway.Middlewares
{
    using AIEngineGateway.Services;
#nullable disable
    public class RateLimitMiddleware
    {
        private ILogger<RateLimitMiddleware> _logger;
        private RequestDelegate _next;
        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
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
