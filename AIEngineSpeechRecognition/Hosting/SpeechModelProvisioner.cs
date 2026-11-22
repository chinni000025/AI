using AIEngineConnectivity.Constants;
using Microsoft.Extensions.Logging;
using Whisper.net.Ggml;

namespace AIEngineSpeechRecognition.Hosting;

public static class SpeechModelProvisioner
{
    private static readonly SemaphoreSlim _downloadLock = new(1, 1);

    public static async Task EnsureModelsReadyAsync(string? baseDirectory = null, ILogger? logger = null)
    {
        baseDirectory ??= AppDomain.CurrentDomain.BaseDirectory;
        var modelsDir = Path.Combine(baseDirectory, WhisperConstants.ModelsFolder);

        if (!Directory.Exists(modelsDir))
        {
            Directory.CreateDirectory(modelsDir);
        }

        var whisperPath = Path.Combine(modelsDir, WhisperConstants.WhisperModel);
        var sileroPath = Path.Combine(modelsDir, WhisperConstants.SilerioVADModel);

        if (File.Exists(whisperPath) && File.Exists(sileroPath))
        {
            return;
        }

        await _downloadLock.WaitAsync();
        try
        {
            // 1. Whisper Model Provisioning
            if (!File.Exists(whisperPath))
            {
                logger?.LogInformation("Whisper model not found at {Path}. Downloading GGML Base English model...", whisperPath);
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                var downloader = new WhisperGgmlDownloader(httpClient);
                using var modelStream = await downloader.GetGgmlModelAsync(GgmlType.BaseEn);
                using var fileWrite = File.Create(whisperPath);
                await modelStream.CopyToAsync(fileWrite);
                logger?.LogInformation("Whisper model downloaded successfully.");
            }

            // 2. Silero VAD Model Provisioning
            if (!File.Exists(sileroPath))
            {
                logger?.LogInformation("Silero VAD model not found at {Path}. Downloading Silero VAD ONNX model...", sileroPath);
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
                using var response = await httpClient.GetAsync(WhisperConstants.SileroVadDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(sileroPath);
                await stream.CopyToAsync(fileStream);
                logger?.LogInformation("Silero VAD ONNX model downloaded successfully.");
            }
        }
        finally
        {
            _downloadLock.Release();
        }
    }

    public static string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
    }
}
