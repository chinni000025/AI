namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;

    public interface IAIProvider
    {
        public Task<AIResponse?> GenerateAsync(AIRequest request);
    }
}