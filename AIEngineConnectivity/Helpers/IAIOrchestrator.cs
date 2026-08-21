using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.Helpers
{
    public interface IAIOrchestrator
    {
        public Task<AIResponse?> ChatAsync(AIRequest aiRequest);
    }
}