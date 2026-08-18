using AIEngineConnectivity.Models;
using AIEngineGateway.EngineInfrastructure.UserRateLimiter;
using System.Security.Claims;

namespace AIEngineGateway.Middlewares
{
    public class SpecificUserRateLimitingMiddleware
    {
        private readonly ILogger<SpecificUserRateLimitingMiddleware> _logger;
        private RequestDelegate _next;
        private readonly UserRateLimiter _UserRateLimiter;

        public SpecificUserRateLimitingMiddleware(RequestDelegate next, ILogger<SpecificUserRateLimitingMiddleware> logger,
            UserRateLimiter userRateLimiter)
        {
            _logger = logger;
            _next = next;
            _UserRateLimiter = userRateLimiter;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.User.Identity?.IsAuthenticated is true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null || !_UserRateLimiter.TryAddRequest(userId))
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
                await _next(context);
            }
        }
    }
}
