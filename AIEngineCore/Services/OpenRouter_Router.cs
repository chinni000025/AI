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
    public class OpenRouter_Router : IAIEngineRouter
    {
        private IAIProvider _AIProvider;

        public OpenRouter_Router(OpenRouterProvider openRouterProvider)
        {
            _AIProvider = openRouterProvider;
        }

        public string AIProviderType => EngineModelProviders.OpenRouter;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
