namespace AIEngineGateway.Middlewares
{
    using System.Text;
    using System.Text.Json;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;

    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IEncryptionService encryptionService)
        {
            // Only decrypt requests marked as encrypted
            if (!context.Request.Headers.TryGetValue("AIEngine-Encryption", out var header) ||
                !string.Equals(header, "true", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // No body
            if (context.Request.ContentLength == 0)
            {
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            string requestBody;

            using (var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true))
            {

                requestBody = await reader.ReadToEndAsync();
            }

            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                await _next(context);
                return;
            }

            EncryptedRequest? encryptedRequest;

            try
            {
                encryptedRequest = JsonSerializer.Deserialize<EncryptedRequest>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid encrypted request.");
                return;
            }

            if (encryptedRequest == null ||
                string.IsNullOrWhiteSpace(encryptedRequest.Payload))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Payload is missing.");
                return;
            }

            string decryptedJson;

            try
            {
                decryptedJson = encryptionService.Decrypt(encryptedRequest.Payload);
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to decrypt request.");
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(decryptedJson);

            context.Request.Body = new MemoryStream(bytes);

            context.Request.ContentLength = bytes.Length;

            context.Request.Body.Position = 0;

            await _next(context);
        }
    }
}
