namespace AIEngineConnectivity.Helpers
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;

    public interface IAIOrchestrator
    {
        public Task<AIResponse?> ChatAsync(AIRequest aiRequest);
    }
}