using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AIEngineCore.Providers
{
    public class CohereProvider : ProviderBase.ProviderBase
    {
        private readonly CohereApiKeyConfiguration _cohereApiKeyConfiguration;
        public CohereProvider(IOptions<CohereApiKeyConfiguration> options)
        {
            _cohereApiKeyConfiguration = options.Value;
        }
        public async override Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            try
            {
                var postUrl = "chat";
                var client = CreateHttpClient(ModelUrl.CohereUrl, _cohereApiKeyConfiguration.ApiKey);
                var payload = CreatePayload(request);
                var jsonDocument = await GenerateJsonContent(payload, client, postUrl);
                string? output = this.GenerateOutput(jsonDocument);
                return CreateResponse(output);
            }
            catch
            {
                throw;
            }
        }

        public override string? GenerateOutput(JsonDocument jsonDocument)
        {
            var content = jsonDocument?.RootElement
                           .GetProperty("message")
                           .GetProperty("content")[0];

            string? output = content?.TryGetProperty("text", out var text) == true ? text.GetString()
                               : content?.TryGetProperty("thinking", out var thinking) == true ? thinking.GetString() : null;
            return output;
        }
    }
}
