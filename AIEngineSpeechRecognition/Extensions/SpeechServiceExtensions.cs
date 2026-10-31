using AIEngineConnectivity.Services;
using AIEngineSpeechRecognition.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIEngineSpeechRecognition.Extensions
{
    public static class SpeechServiceExtensions
    {
        public static IServiceCollection AddEngineSpeechRecognition(this IServiceCollection services)
        {
            services.AddSingleton<IWhisperService, WhisperService>();
            return services;
        }
    }
}