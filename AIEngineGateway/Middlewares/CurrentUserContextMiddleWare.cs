namespace AIEngineGateway.Middlewares
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;
    using System.Security.Claims;
    using System.Transactions;
#nullable disable
    public class CurrentUserContextMiddleWare
    {
        public RequestDelegate _next;
        public CurrentUserContextMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            CurrentUser currentUser = new CurrentUser();

            if (context.User.Identity?.IsAuthenticated == true)
            {
                currentUser.UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                currentUser.UserName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                currentUser.Email = context.User.FindFirst(ClaimTypes.Email)?.Value;
                currentUser.isAuthenticated = context.User.Identity.IsAuthenticated;
                context.Items["CurrentUser"] = currentUser;
            }

            await _next(context);
        }
    }
}
