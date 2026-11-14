
using AIEngineRedis.Abstraction;
using AIEngineRedis.Configurations;
using AIEngineRedis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AIEngineRedis.Extensions;

public static class RedisServiceExtensions
{
    public static IServiceCollection AddAIEngineRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis connection string is required.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;

            var configurationOptions = ConfigurationOptions.Parse(options.ConnectionString);
            configurationOptions.ConnectTimeout = options.Timeout;
            configurationOptions.SyncTimeout = options.SyncTimeout;
            configurationOptions.ConnectRetry = options.ConnectRetry;
            configurationOptions.AbortOnConnectFail = options.AbortOnConnectFail;
            configurationOptions.Ssl = options.Ssl;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddSingleton<RedisConnectionManager>();
        services.AddStackExchangeRedisCache(options =>
        {
            var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();

            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName = string.IsNullOrWhiteSpace(redisOptions.InstanceName)
                                    ? "AIEngine:" : redisOptions.InstanceName.TrimEnd(':') + ":";
        });

        services.AddHybridCache();
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", tags: new[] { "redis" });
        return services;
    }
}
