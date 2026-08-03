namespace AIEngineCore.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Providers;
#nullable disable
    public class CohereRouter : IAIEngineRouter
    {
        private IAIProvider _AIProvider;

        public CohereRouter(CohereProvider cohereProvider)
        {
            _AIProvider = cohereProvider;
        }
        public string AIProviderType => EngineModelProviders.Cohere;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
