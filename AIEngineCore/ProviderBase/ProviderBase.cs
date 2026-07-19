namespace AIEngineCore.ProviderBase
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using Google.GenAI;
    using System.Net.Http.Headers;
    using System.Text.Json;

    public abstract class ProviderBase : IAIProvider
    {
        public abstract Task<AIResponse?> GenerateAsync(AIRequest request);

        public HttpClient CreateHttpClient(String baseAddress, String? bearerToken = null, int timeOut = 5)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromMinutes(timeOut)
            };

            if (!string.IsNullOrWhiteSpace(bearerToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        }

        public ChatPayload CreatePayload(AIRequest request, bool isStreaming = false)
        {
            var messages = new List<object>();
            foreach (var message in request.ConversationHistory)
            {
                messages.Add(new { role = message.Role.ToLower(), content = message.Content });
            }

            messages.Add(new { role = EngineRoles.User.ToLower(), content = request.Prompt });
            return new ChatPayload
            {
                Model = request.Model,
                Messages = messages,
                Stream = isStreaming
            };
        }

        public async Task<JsonDocument?> GenerateJsonContent(ChatPayload payload, HttpClient client, string postUrl = "chat/completions")
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(postUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);
            return jsonDocument;
        }

        public virtual String? GenerateOutput(JsonDocument jsonDocument)
        {
            if (jsonDocument is null) return null;
            return jsonDocument?.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        public AIResponse CreateResponse(string? output)
        {
            return new AIResponse
            {
                Output = output!,
                Success = !string.IsNullOrEmpty(output)
            };
        }
    }
}
