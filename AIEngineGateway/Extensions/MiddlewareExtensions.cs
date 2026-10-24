using AIEngineGateway.Middlewares;

namespace AIEngineGateway.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void AddEngineMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngular");
            app.UseMiddleware<ServerRateLimitingMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<SpecificUserRateLimitingMiddleware>();
            app.UseMiddleware<CurrentUserContextMiddleWare>();
            app.UseRateLimiter();
            app.UseAuthorization();
            app.UseMiddleware<EncryptionMiddleware>();
        }
    }
}
