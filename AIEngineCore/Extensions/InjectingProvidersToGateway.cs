namespace AIEngineCore.Extensions
{
    using AIEngineConnectivity.EngineCore;
    using AIEngineCore.EngineCore;
    using AIEngineCore.EngineNotifications;
    using AIEngineCore.Providers;
    using AIEngineCore.Services;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public static class InjectingProvidersToGateway
    {
        public static IServiceCollection AddEngineCoreDependencies(this IServiceCollection services)
        {
            InitializeEngineCore(services);
            InitailzeProviders(services);
            AddingHostingServices(services);
            InitializingRouters(services);
            return services;
        }

        public static void InitializeEngineCore(IServiceCollection services)
        {
            services.AddSingleton<EngineEventPublisher>();
            services.AddSingleton<IEngineBus, EngineBus>();
            services.AddSingleton<IEmbeddedResourceProvider>(sp =>
              new EmbeddedResourceProvider(Assembly.GetEntryAssembly()
              ?? typeof(InjectingProvidersToGateway).Assembly));
            services.AddSingleton<ITemplateProvider, TemplateProvider>();
            services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
            services.AddSingleton(typeof(IEngineQueue<>), typeof(EngineQueue<>));
        }

        public static void InitailzeProviders(IServiceCollection services)
        {
            services.AddSingleton<GeminiProvider>();
            services.AddSingleton<GroqProvider>();
            services.AddSingleton<HuggingFaceProvider>();
            services.AddSingleton<OllamaProviders>();
            services.AddSingleton<OpenRouterProvider>();
            services.AddSingleton<CohereProvider>();
        }

        public static void AddingHostingServices(IServiceCollection services)
        {
            services.AddHostedService<EngineDispatcher>();
            services.AddHostedService<EngineEmailWorker>();
            services.AddHostedService<EngineEmailRetryWorker>();
        }

        public static void InitializingRouters(IServiceCollection services)
        {
            services.AddSingleton<IAIEngineRouter, CohereRouter>();
            services.AddSingleton<IAIEngineRouter, GeminiRouter>();
            services.AddSingleton<IAIEngineRouter, GroqRouter>();
            services.AddSingleton<IAIEngineRouter, HuggingFaceRouter>();
            services.AddSingleton<IAIEngineRouter, Ollamarouter>();
            services.AddSingleton<IAIEngineRouter, OpenRouter_Router>();
            services.AddSingleton<IEngineNotificationRouter, EngineEmailRouter>();
        }
    }
}
