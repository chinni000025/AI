namespace AIEngineCore.Extensions
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;

    public class AIExtension
    {
        IAIProvider aIProvider;

        public AIExtension(IAIProvider aIProvider)
        {
            this.aIProvider = aIProvider;
        }

        public async Task<AIResponse?> GenerateResponse(AIRequest aIRequest)
        {
            return await aIProvider.GenerateAsync(aIRequest);
        }
    }
}