using System.Text.Json;
using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using Microsoft.Extensions.Options;

namespace AIEngineCore.Providers
{
    public class OpenRouterProvider : ProviderBase.ProviderBase
    {
        private readonly OpenRouterAPiKeyConfiguration _openRouterAPiKeyConfiguration;
        public OpenRouterProvider(IOptions<OpenRouterAPiKeyConfiguration> options)
        {
            _openRouterAPiKeyConfiguration = options.Value;
        }

        public override async Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            try
            {
                var client = CreateHttpClient(ModelUrl.ORUrl, _openRouterAPiKeyConfiguration.ApiKey);
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