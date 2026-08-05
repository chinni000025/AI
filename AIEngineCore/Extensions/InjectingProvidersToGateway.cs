namespace AIEngineCore.Extensions
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineCore.Providers;
    using AIEngineCore.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class InjectingProvidersToGateway
    {
        public static IServiceCollection AddEngineCoreDependencies(this IServiceCollection services)
        {
            services.AddSingleton<GeminiProvider>();
            services.AddSingleton<GroqProvider>();
            services.AddSingleton<HuggingFaceProvider>();
            services.AddSingleton<OllamaProviders>();
            services.AddSingleton<OpenRouterProvider>();
            services.AddSingleton<CohereProvider>();
            services.AddSingleton<IAIEngineRouter, CohereRouter>();
            services.AddSingleton<IAIEngineRouter, GeminiRouter>();
            services.AddSingleton<IAIEngineRouter, GroqRouter>();
            services.AddSingleton<IAIEngineRouter, HuggingFaceRouter>();
            services.AddSingleton<IAIEngineRouter, Ollamarouter>();
            services.AddSingleton<IAIEngineRouter, OpenRouter_Router>();
            return services;
        }
    }
}
