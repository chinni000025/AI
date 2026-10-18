using AIEngineConnectivity.Models;
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

            try
            {
                await _next(context);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing the HTTP request. " +
                "CorrelationId: {CorrelationId}", correlationId);
                if (context.RequestAborted.IsCancellationRequested)
                {
                    return;
                }
                await HandleExceptionAsync(context, ex, correlationId);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, string correlationId)
        {
            try
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new EngineErrorResponse
                {
                    StatusCode = GetStatusCode(ex),
                    ErrorMessage = "An unexpected error occurred.",
                    CorrelationId = correlationId
                });
            }
            catch (Exception e)
            {
                _logger.LogWarning(
                   e, "Failed to write the error response. " +
                   "CorrelationId: {CorrelationId}",
                   correlationId);
            }
        }

        private int GetStatusCode(Exception ex)
        {
            var statusCode = ex switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            return statusCode;
        }
    }
}