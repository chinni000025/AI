using System.Text.RegularExpressions;
using Whisper.net;

namespace AIEngineSpeechRecognition.Transcription;

public sealed class WhisperNetTranscriber : IAsyncDisposable
{
    private readonly WhisperFactory _factory;
    private readonly SemaphoreSlim _semaphore;

    public WhisperNetTranscriber(string modelPath, int maxConcurrent = 1)
    {
        _factory = WhisperFactory.FromPath(modelPath);
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<string> TranscribeAsync(ReadOnlyMemory<float> audio16kMono, bool isFinal, CancellationToken cancellationToken = default)
    {
        if (audio16kMono.Length < 3200) return string.Empty; // Skip < 200ms noise

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            using var processor = _factory.CreateBuilder()
                .WithThreads(Math.Min(Environment.ProcessorCount, 6))
                .WithLanguage("en")
                .WithTemperature(0f)
                .WithNoContext()
                .Build();

            var sb = new System.Text.StringBuilder();
            try
            {
                await foreach (var segment in processor.ProcessAsync(audio16kMono.ToArray(), cancellationToken))
                {
                    if (!string.IsNullOrWhiteSpace(segment.Text))
                    {
                        sb.Append(segment.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[Whisper] Inference failed for {Samples} samples", audio16kMono.Length);
            }

            // Strip hallucinated bracket tokens like [BLANK_AUDIO], *slurp*, (music), etc.
            string raw = sb.ToString();
            string clean = Regex.Replace(raw, @"\[.*?\]|\(.*?\)|(\*.*?\*)", "").Trim();

            // Filter out common YouTube subtitle hallucination artifacts
            if (audio16kMono.Length < 32000)
            {
                string lower = clean.ToLowerInvariant().TrimEnd('.', '!', '?', ',', ' ');
                if (lower == "thanks for watching" || lower == "thank you for watching" ||
                    lower == "please subscribe" || lower == "subscribe to my channel" ||
                    lower == "subtitles by" || lower == "translated by")
                {
                    return string.Empty;
                }
            }

            return clean;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _factory.Dispose();
        _semaphore.Dispose();
        return ValueTask.CompletedTask;
    }
}