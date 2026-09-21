using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Helpers;
using AIEngineConnectivity.Models;
using Microsoft.Extensions.Logging;

namespace AIEngineCore.Services
{
#nullable disable

    public class AIOrchestrator : IAIOrchestrator
    {
        private ILogger<AIOrchestrator> _logger;
        private readonly IReadOnlyDictionary<string, IAIEngineRouter> _Router;

        public AIOrchestrator(ILogger<AIOrchestrator> logger)
        {
            _logger = logger;
        }

        public async Task<AIResponse?> ChatAsync(AIRequest aiRequest)
        {
            try
            {
                AIResponse response = null;
                if (_Router.TryGetValue(aiRequest.Provider, out var provider))
                {
                    response = await provider.GenerateAIResponse(aiRequest);
                }
                return new AIResponse
                {
                    Output = response?.Output,
                    Success = response.Success
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}