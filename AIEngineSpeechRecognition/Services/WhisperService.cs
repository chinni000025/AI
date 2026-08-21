using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using System;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace AIEngineSpeechRecognition.Services
{
    public class WhisperService : IDisposable, IWhisperService
    {
        private readonly string _modelPath;
        private readonly string _ffmpegPath; // for 16khz frequency conversion.
        private WhisperFactory _whisperFactory;
        private bool _isInitialize;
        private WhisperProcessor _processor;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);
        public WhisperService()
        {
            // Application Running Path.
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // directory//folder//filename
            _modelPath = Path.Combine(baseDirectory, WhisperConstants.ModelsFolder, WhisperConstants.WhisperModel);
            _ffmpegPath = Path.Combine(baseDirectory, WhisperConstants.AudioConverter);
        }
        public async Task InitializeAsync()
        {
            await EnsureDependenciesReadAsync();
        }

        public async Task EnsureDependenciesReadAsync()
        {
            if (_isInitialize)
                return;
            if (!Directory.Exists(_ffmpegPath) || !File.Exists(Path.Combine(_ffmpegPath, WhisperConstants.AudioExecutionFile)))
            {
                Directory.CreateDirectory(_ffmpegPath);
                FFmpeg.SetExecutablesPath(_ffmpegPath);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _ffmpegPath);
            }
            else
            {
                FFmpeg.SetExecutablesPath(_ffmpegPath);
            }

            var modelDirectory = Path.GetDirectoryName(_modelPath)!;
            if (!Directory.Exists(modelDirectory))
                Directory.CreateDirectory(modelDirectory);

            if (!File.Exists(_modelPath))
            {
                using var httpClient = new HttpClient();
                var downloader = new WhisperGgmlDownloader(httpClient);

                using var modelStream = await downloader.GetGgmlModelAsync(GgmlType.BaseEn);
                using var fileWrite = File.Create(_modelPath);

                await modelStream.CopyToAsync(fileWrite);
            }

            if (!File.Exists(_modelPath))
                throw new Exception("Model file not found after download.");

            _whisperFactory = WhisperFactory.FromPath(_modelPath);
            _isInitialize = true;
        }

        public async Task<string> TranscribeAudioAsync(string inputFilePath, CancellationToken cancellationToken)
        {
            await EnsureDependenciesReadAsync();
            await _semaphore.WaitAsync();

            string tempWavPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
            try
            {   //convert to 16khz frequency per sec using pluse code modulation.__--__--__--__-- like this manner __ represents 0 and -- represents 1.
                await FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{inputFilePath}\" {WhisperConstants.SamplingRate} {WhisperConstants.Channel} {WhisperConstants.AudioCode} \"{tempWavPath}\"")
                    .Start();
                _processor = _whisperFactory.CreateBuilder().WithLanguage(WhisperConstants.Language).Build();
                StringBuilder stringBuilder = new StringBuilder();
                using var fileRead = File.OpenRead(tempWavPath);
                await foreach (var result in _processor.ProcessAsync(fileRead, cancellationToken))
                {
                    stringBuilder.Append(result.Text).Append(" ");
                }
                return stringBuilder.ToString().Trim();
            }
            finally
            {
                _semaphore.Release(); // prevents dead lock.
                //removing Temp file
                if (File.Exists(tempWavPath))
                    File.Delete(tempWavPath);
            }

        }
        public void Dispose()
        {
            _whisperFactory?.Dispose();
            _semaphore?.Dispose();
            _processor?.Dispose();

        }
    }
}
