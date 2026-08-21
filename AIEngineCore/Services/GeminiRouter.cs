using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;
using AIEngineCore.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineCore.Services
{
#nullable disable
    public class GeminiRouter : IAIEngineRouter
    {
        private IAIProvider _AIProvider;

        public GeminiRouter(GeminiProvider geminiProvider)
        {
            _AIProvider = geminiProvider;
        }
        public string AIProviderType => EngineModelProviders.Gemini;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
