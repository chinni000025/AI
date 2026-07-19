namespace AIEngineCore.Extensions
{
    using AIEngineCore.Providers;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class InjectingProvidersToGateway
    {
        public static IServiceCollection AddEngineCoreDependencies(this IServiceCollection services)
        {
            services.AddScoped<GeminiProvider>();
            services.AddScoped<GroqProvider>();
            services.AddScoped<HuggingFaceProvider>();
            services.AddScoped<OllamaProviders>();
            services.AddScoped<OpenRouterProvider>();
            services.AddScoped<CohereProvider>();
            return services;
        }
    }
}
