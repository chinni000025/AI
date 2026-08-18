namespace AIEngineGateway.Extensions
{
    using AIEngineGateway.Hub;
    using AIEngineGateway.Middlewares;
    public static class MiddlewareExtensions
    {
        public static void AddEngineMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngular");
            app.UseMiddleware<ServerRateLimitingMiddleware>();
            app.UseMiddleware<SpecificUserRateLimitingMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<CurrentUserContextMiddleWare>();
            app.UseAuthorization();
            app.UseMiddleware<EncryptionMiddleware>();
        }
    }
}
