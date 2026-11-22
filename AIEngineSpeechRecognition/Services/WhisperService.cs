using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using AIEngineSpeechRecognition.Hosting;
using System.Text;
using Whisper.net;

namespace AIEngineSpeechRecognition.Services
{
    public class WhisperService : IDisposable, IWhisperService
    {
        private string _modelPath;
        private WhisperFactory? _whisperFactory;
        private bool _isInitialize;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);

        public WhisperService()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var primaryModel = Path.Combine(baseDirectory, WhisperConstants.ModelsFolder, WhisperConstants.WhisperModel);
            if (File.Exists(primaryModel))
            {
                _modelPath = primaryModel;
            }
            else
            {
                _modelPath = Path.Combine(baseDirectory, WhisperConstants.ModelsFolder, "ggml-base.en.bin");
            }
        }

        public async Task InitializeAsync()
        {
            await EnsureDependenciesReadAsync();
        }

        public async Task EnsureDependenciesReadAsync()
        {
            if (_isInitialize)
                return;

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            await SpeechModelProvisioner.EnsureModelsReadyAsync(baseDirectory);

            if (!File.Exists(_modelPath))
            {
                var fallback = Path.Combine(baseDirectory, "Models", "ggml-base.en.bin");
                if (File.Exists(fallback))
                {
                    _modelPath = fallback;
                }
                else
                {
                    throw new FileNotFoundException("Whisper model file not found after provisioning.", _modelPath);
                }
            }

            _whisperFactory = WhisperFactory.FromPath(_modelPath);
            _isInitialize = true;
        }

        public async Task<string> TranscribeAudioAsync(string inputFilePath, CancellationToken cancellationToken)
        {
            await EnsureDependenciesReadAsync();
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                using var processor = _whisperFactory!.CreateBuilder()
                    .WithLanguage(WhisperConstants.Language)
                    .WithSingleSegment()
                    .WithTemperature(0f)
                    .Build();

                var stringBuilder = new StringBuilder();
                using var fileRead = File.OpenRead(inputFilePath);

                await foreach (var result in processor.ProcessAsync(fileRead, cancellationToken))
                {
                    if (!string.IsNullOrWhiteSpace(result.Text))
                    {
                        stringBuilder.Append(result.Text).Append(' ');
                    }
                }

                return stringBuilder.ToString().Trim();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _whisperFactory?.Dispose();
            _semaphore?.Dispose();
        }
    }
}
