using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;
using AIEngineCore.Providers;

namespace AIEngineCore.Services
{
#nullable disable

    public class GroqRouter : IAIEngineRouter
    {
        private IAIProvider _AIProvider;

        public GroqRouter(GroqProvider groqProvider)
        {
            _AIProvider = groqProvider;
        }

        public string AIProviderType => EngineModelProviders.Groq;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
