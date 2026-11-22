using System.Text.RegularExpressions;
using Whisper.net;

namespace AIEngineSpeechRecognition.Transcription;

public sealed class WhisperNetTranscriber : IAsyncDisposable
{
    private readonly WhisperFactory _factory;
    private readonly SemaphoreSlim _semaphore;

    public WhisperNetTranscriber(string modelPath, int maxConcurrent = 2)
    {
        _factory = WhisperFactory.FromPath(modelPath);
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async Task<string> TranscribeAsync(ReadOnlyMemory<float> audio16kMono, bool isFinal, CancellationToken cancellationToken = default)
    {
        if (audio16kMono.Length < 4800) return string.Empty; // Skip < 300ms noise

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            using var processor = _factory.CreateBuilder()
                .WithThreads(Math.Min(Environment.ProcessorCount, 8))
                .WithLanguage("en")
                .WithSingleSegment()
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

            // Filter out common YouTube subtitle hallucination artifacts on short speech
            if (audio16kMono.Length < 32000) // < 2 seconds
            {
                string lower = clean.ToLowerInvariant().TrimEnd('.', '!', '?', ',', ' ');
                if (lower == "bye" || lower == "cheers" || lower == "thanks for watching" ||
                    lower == "thank you" || lower == "please subscribe" || lower == "subscribe" ||
                    lower == "you" || lower == "slurp")
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