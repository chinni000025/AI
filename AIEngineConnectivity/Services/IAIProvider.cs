using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;

namespace AIEngineConnectivity.Services
{
    public interface IAIProvider
    {
        public Task<AIResponse?> GenerateAsync(AIRequest request);
    }
}