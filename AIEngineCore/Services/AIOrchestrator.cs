using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Helpers;
using AIEngineConnectivity.Models;
using AIEngineCore.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace AIEngineCore.Services
{
#nullable disable

    public class AIOrchestrator : IAIOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        private ILogger<AIOrchestrator> _logger;
        private IAIEngineRouter _AIEngineRouter;
        private readonly IReadOnlyDictionary<string, IAIEngineRouter> _Router;

        public AIOrchestrator(IServiceProvider serviceProvider,
            ILogger<AIOrchestrator> logger, IEnumerable<IAIEngineRouter> aIEngineRouter)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _Router = aIEngineRouter.ToDictionary(a => a.AIProviderType, StringComparer.OrdinalIgnoreCase);
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