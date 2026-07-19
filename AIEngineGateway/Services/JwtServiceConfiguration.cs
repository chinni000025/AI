namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Models;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    public static class JwtServiceConfiguration
    {
        public static void AddingJWtService(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtSection = configuration.GetSection("Jwt").Get<JWTConfiguration>();
            if (JwtSection is null)
                throw new Exception("Engine Failed to Ignition");

            if (string.IsNullOrEmpty(JwtSection.Key))
                throw new Exception("Invalid Engine Key.");

            var encodingKey = Convert.FromBase64String(JwtSection.Key);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = JwtSection.Audience,
                    ValidIssuer = JwtSection.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(encodingKey),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var acessToken = context.Request.Query[AuthConstants.EngineIgnition];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(acessToken) &&
                            path.StartsWithSegments("/api/notificationHub"))
                        {
                            context.Token = acessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = AuthConstants.EnginesVerification;
                options.Cookie.Name = "Engine-Key-Token";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
        }
    }
}
