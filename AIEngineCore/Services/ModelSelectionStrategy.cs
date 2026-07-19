namespace AIEngineCore.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Services;
    using AIEngineCore.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class ModelSelectionStrategy
    {
        private readonly IServiceProvider _serviceProvider;

        public ModelSelectionStrategy(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Func<IAIProvider> GetModelContext(string model)
        {
            return model.ToLowerInvariant() switch
            {
                EngineModelProviders.Ollama
                        => () => _serviceProvider.GetRequiredService<OllamaProviders>(),

                EngineModelProviders.Gemini
                         => () => _serviceProvider.GetRequiredService<GeminiProvider>(),

                EngineModelProviders.Groq
                        => () => _serviceProvider.GetRequiredService<GroqProvider>(),

                EngineModelProviders.HuggingFace
                        => () => _serviceProvider.GetRequiredService<HuggingFaceProvider>(),

                EngineModelProviders.OpenRouter
                        => () => _serviceProvider.GetRequiredService<OpenRouterProvider>(),
                
                EngineModelProviders.Cohere
                        => () => _serviceProvider.GetRequiredService<CohereProvider>(),

                _ => throw new Exception($"Model {model} Doesn't Exists in Engine")
            };
        }
    }
}
