using AIEngineConnectivity.Models;
using Serilog.Context;
namespace AIEngineGateway.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var incomingCorrelationId = context.Request.Headers[CorrelationIdHeader].ToString();

            var correlationId = Guid.TryParse(incomingCorrelationId, out var parsedCorrelationId)
                    ? parsedCorrelationId.ToString("D") : Guid.NewGuid().ToString("D");

            context.TraceIdentifier = correlationId;
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _next(context);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    _logger.LogInformation("Request cancelled by client. CorrelationId: {CorrelationId}", correlationId);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 499; // Client Closed Request
                    }
                }
                catch (Exception ex)
                {
                    if (context.Response.HasStarted)
                    {
                        _logger.LogWarning(ex, "Response already started. Cannot write error response. CorrelationId: {CorrelationId}", correlationId);
                        throw;
                    }

                    var statusCode = GetStatusCode(ex);

                    if (statusCode >= 500)
                    {
                        _logger.LogError(ex, "Unhandled server error processing request {Path}. Status: {StatusCode}, CorrelationId: {CorrelationId}",
                            context.Request.Path, statusCode, correlationId);
                    }
                    else
                    {
                        _logger.LogWarning("Client error processing request {Path}: {Message}. Status: {StatusCode}, CorrelationId: {CorrelationId}",
                            context.Request.Path, ex.Message, statusCode, correlationId);
                    }

                    await HandleExceptionAsync(context, ex, correlationId, statusCode);
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, string correlationId, int statusCode)
        {
            try
            {
                var clientMessage = statusCode >= 500
                    ? "An unexpected error occurred. Please contact support."
                    : ex.Message;

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new EngineErrorResponse
                {
                    StatusCode = statusCode,
                    ErrorMessage = clientMessage,
                    CorrelationId = correlationId
                });
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to write the error response. CorrelationId: {CorrelationId}", correlationId);
            }
        }

        private int GetStatusCode(Exception ex)
        {
            var statusCode = ex switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                TimeoutException => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status500InternalServerError
            };
            return statusCode;
        }
    }
}