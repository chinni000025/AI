namespace AIEngineCore.Services
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Helpers;
    using AIEngineConnectivity.Models;
    using AIEngineCore.Extensions;
    using Microsoft.Extensions.Logging;
    using System;
#nullable disable

    public class AIOrchestrator : IAIOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        ModelSelectionStrategy modelStrategy;
        private ILogger<AIOrchestrator> _logger;

        public AIOrchestrator(IServiceProvider serviceProvider,
            ILogger<AIOrchestrator> logger)
        {
            _serviceProvider = serviceProvider;
            modelStrategy = new ModelSelectionStrategy(_serviceProvider);
            _logger = logger;
        }

        public async Task<AIResponse?> ChatAsync(AIRequest aiRequest)
        {
            try
            {
                AIExtension aIService = new AIExtension(modelStrategy.GetModelContext(aiRequest.Provider)());
                var response = await aIService.GenerateResponse(aiRequest);
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