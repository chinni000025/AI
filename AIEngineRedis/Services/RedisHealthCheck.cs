using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AIEngineRedis.Services;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer connection, ILogger<RedisHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connection.IsConnected)
            {
                return HealthCheckResult.Degraded("Redis connection is currently unavailable.");
            }

            var latency = await _connection.GetDatabase().PingAsync().WaitAsync(cancellationToken);

            _logger.LogDebug("Redis health check succeeded. Response time: {Latency} ms", latency.TotalMilliseconds);

            return HealthCheckResult.Healthy("Redis is operational.",
                new Dictionary<string, object>
                {
                    ["ResponseTimeMs"] = latency.TotalMilliseconds
                });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Redis health check failed.");

            return HealthCheckResult.Degraded("Redis is currently unavailable.", exception);
        }
    }
}
