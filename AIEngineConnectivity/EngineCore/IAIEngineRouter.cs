using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.EngineCore
{
    public interface IAIEngineRouter
    {
        public string AIProviderType { get; }
        public Task<AIResponse> GenerateAIResponse(AIRequest aIRequest);
    }
}
