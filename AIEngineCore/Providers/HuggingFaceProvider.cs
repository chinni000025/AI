namespace AIEngineCore.Providers
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;
    using Microsoft.Extensions.Options;
    using System;
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class HuggingFaceProvider : ProviderBase.ProviderBase
    {
        private readonly HuggingFaceApiKeyConfiguration _huggingFaceApiKeyConfiguration;
        public HuggingFaceProvider(IOptions<HuggingFaceApiKeyConfiguration> options)
        {
            _huggingFaceApiKeyConfiguration = options.Value;
        }

        public override async Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            try
            {
                var client = CreateHttpClient(ModelUrl.HfUrl, _huggingFaceApiKeyConfiguration.ApiKey);
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
