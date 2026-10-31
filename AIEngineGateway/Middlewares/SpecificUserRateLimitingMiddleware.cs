using AIEngineConnectivity.Helpers;
using AIEngineConnectivity.Services;
using System.Security.Claims;

namespace AIEngineGateway.Middlewares
{
    public class SpecificUserRateLimitingMiddleware
    {
        private readonly ILogger<SpecificUserRateLimitingMiddleware> _logger;
        private RequestDelegate _next;
        private readonly IUserRateLimiter _userRateLimiter;

        public SpecificUserRateLimitingMiddleware(RequestDelegate next, ILogger<SpecificUserRateLimitingMiddleware> logger,
            IUserRateLimiter userRateLimiter)
        {
            _logger = logger;
            _next = next;
            _userRateLimiter = userRateLimiter;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.GetEndpoint()?.Metadata.GetMetadata<SkipLeakyBucketAttribute>() != null)
            {
                await _next(context);
                return;
            }
            if (context.User.Identity?.IsAuthenticated is true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null || !_userRateLimiter.TryAddRequest(userId))
                {
                    if (userId is null)
                    {
                        _logger.LogError("User Id is null");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Bad Request");
                        return;
                    }
                    context.Response.StatusCode = 429;
                    await context.Response.WriteAsync("Too Many Request");
                    _logger.LogError("Client Request To Many Requests");
                    return;
                }
            }
            await _next(context);
        }
    }
}
