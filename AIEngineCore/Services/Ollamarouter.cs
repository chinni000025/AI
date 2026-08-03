namespace AIEngineCore.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Providers;
    using System;
    using System.Collections.Generic;
    using System.Text;

#nullable disable
    public class Ollamarouter : IAIEngineRouter
    {
        private IAIProvider _AIProvider;
        public Ollamarouter(OllamaProviders OllamaProvider)
        {
            _AIProvider = OllamaProvider;
        }
        public string AIProviderType => EngineModelProviders.Ollama;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
