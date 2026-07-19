namespace AIEngineGateway.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _environment = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), geolocation=()";

            if (_environment.IsDevelopment())
            {
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "img-src 'self' data: blob:; " +
                    "connect-src 'self' ws: wss: http://localhost:* https://localhost:*; " +
                    "font-src 'self' data: https://fonts.gstatic.com; " +
                    "frame-ancestors 'none'";
            }
            else
            {
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self'; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "img-src 'self' data: blob:; " +
                    "connect-src 'self'; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "frame-ancestors 'none'; " +
                    "upgrade-insecure-requests";
            }

            await _next(context);
        }
    }
}
