using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using AIEngineConnectivity.SpeechRecognition;
using AIEngineSpeechRecognition.Hosting;
using AIEngineSpeechRecognition.Services;
using AIEngineSpeechRecognition.Sessions;
using AIEngineSpeechRecognition.Transcription;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIEngineSpeechRecognition.Extensions
{
    public static class SpeechServiceExtensions
    {
        public static IServiceCollection AddEngineSpeechRecognition(this IServiceCollection services)
        {
            services.AddSingleton<IWhisperService, WhisperService>();

            // Live STT components with automatic model provisioning for new deployments
            services.AddSingleton(sp =>
            {
                var logger = sp.GetService<ILogger<WhisperNetTranscriber>>();
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var modelPath = Path.Combine(baseDirectory, WhisperConstants.ModelsFolder, WhisperConstants.WhisperModel);

                // Ensure Whisper and Silero VAD models are downloaded on the new environment
                SpeechModelProvisioner.EnsureModelsReadyAsync(baseDirectory, logger).GetAwaiter().GetResult();

                return new WhisperNetTranscriber(modelPath, maxConcurrent: 1);
            });

            services.AddSingleton<IStreamingSttSessionFactory, StreamingSttSessionFactory>();

            return services;
        }
    }
}
