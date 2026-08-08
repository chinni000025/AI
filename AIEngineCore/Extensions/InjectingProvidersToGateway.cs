namespace AIEngineCore.Extensions
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineCore.EngineCore;
    using AIEngineCore.EngineNotifications;
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
            services.AddSingleton<IEmbeddedResourceProvider>(sp =>
                new EmbeddedResourceProvider(typeof(EmbeddedResourceProvider).Assembly));
            services.AddSingleton<ITemplateProvider, TemplateProvider>();
            services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
            services.AddSingleton(typeof(IEngineQueue<>), typeof(EngineQueue<>));
            services.AddHostedService<EngineDispatcher>();
            services.AddHostedService<EngineEmailWorker>();
            services.AddHostedService<EngineEmailRetryWorker>();
            return services;
        }
    }
}
