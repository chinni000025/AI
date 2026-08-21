using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;

namespace AIEngineCore.Providers
{
    public class OllamaProviders : ProviderBase.ProviderBase
    {
        public override async Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            try
            {
                var client = CreateHttpClient(ModelUrl.OllamaUrl);
                var payload = CreatePayload(request);
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/chat", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = System.Text.Json.JsonDocument.Parse(responseBody);
                var output = jsonDocument.RootElement.GetProperty("message").GetProperty("content").GetString();
                return CreateResponse(output);
            }
            catch
            {
                throw;
            }
        }
    }
}