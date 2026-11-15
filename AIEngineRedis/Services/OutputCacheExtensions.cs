using AIEngineConnectivity.Helpers;
using AIEngineRedis.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace AIEngineRedis.Services
{
    public static class OutputCacheExtensions
    {
        public static IServiceCollection AddEngineOutputCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Connect Output Cache store directly to Redis
            services.AddStackExchangeRedisOutputCache(options =>
            {
                var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = "AIEngine_OutputCache:";
            });

            services.AddOutputCache(options =>
            {
                // Policy 1: User-Specific Data (Conversations, Projects, Files)
                // - 5 minute TTL
                // - Isolated per User ID claim
                // - Varies by query parameters
                options.AddPolicy(CachePolicies.UserCachePolicy, builder => builder
                    .Expire(TimeSpan.FromMinutes(5))
                    .Tag(PoliciesTags.UserDataTag)
                    .SetVaryByQuery("*")
                    .VaryByValue(context =>
                    {
                        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var userValue = !string.IsNullOrWhiteSpace(userId) ? $"usr_{userId}" : "anon";
                        return new KeyValuePair<string, string>("userId", userValue);
                    }));

                // Policy 2: Global Shared Data (AI Models List, Public Keys)
                // - 15 minute TTL
                // - Shared across all users
                options.AddPolicy(CachePolicies.GlobalCachePolicy, builder => builder
                    .Expire(TimeSpan.FromMinutes(15))
                    .Tag(PoliciesTags.GlobalDataTag)
                    .SetVaryByQuery("*"));

                // Policy 3: Fast-refreshing Status Data (Engine Status)
                // - 30 seconds TTL
                options.AddPolicy(CachePolicies.ShortLivedCachePolicy, builder => builder
                    .Expire(TimeSpan.FromSeconds(30))
                    .Tag(PoliciesTags.StatusDataTag));
            });

            return services;
        }

        public static IApplicationBuilder UseEngineOutputCache(this IApplicationBuilder app)
        {
            return app.UseOutputCache();
        }
    }
}