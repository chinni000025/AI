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
    public class HuggingFaceRouter : IAIEngineRouter
    {
        private IAIProvider _AIProvider;

        public HuggingFaceRouter(HuggingFaceProvider huggingFaceProvider)
        {
            _AIProvider = huggingFaceProvider;
        }
        public string AIProviderType => EngineModelProviders.HuggingFace;

        public async Task<AIResponse> GenerateAIResponse(AIRequest aIRequest)
        {
            return await _AIProvider.GenerateAsync(aIRequest);
        }
    }
}
