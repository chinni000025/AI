using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using Microsoft.Extensions.Options;

namespace AIEngineCore.Providers
{
    public class GroqProvider : ProviderBase.ProviderBase
    {
        private readonly GroqApiKeyConfiguration _groqApiKeyConfiguration;
        public GroqProvider(IOptions<GroqApiKeyConfiguration> options)
        {
            _groqApiKeyConfiguration = options.Value;
        }

        public override async Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            try
            {
                var client = CreateHttpClient(ModelUrl.GroqUrl, _groqApiKeyConfiguration.ApiKey);
                var payload = CreatePayload(request);
                var jsonDocument = await GenerateJsonContent(payload, client);
                var output = GenerateOutput(jsonDocument);
                return CreateResponse(output);
            }
            catch
            {
                throw;
            }
        }
    }
}