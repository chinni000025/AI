using AIEngineConnectivity.SpeechRecognition;
using AIEngineSpeechRecognition.Configuration;
using AIEngineSpeechRecognition.Hosting;
using AIEngineSpeechRecognition.Transcription;
using AIEngineSpeechRecognition.Vad;
using Microsoft.Extensions.Options;

namespace AIEngineSpeechRecognition.Sessions;

public sealed class StreamingSttSessionFactory : IStreamingSttSessionFactory
{
    private readonly SpeechRecognitionOptions _options;
    private readonly WhisperNetTranscriber _transcriber;

    public StreamingSttSessionFactory(IOptions<SpeechRecognitionOptions> options, WhisperNetTranscriber transcriber)
    {
        _options = options?.Value ?? new SpeechRecognitionOptions();
        _transcriber = transcriber;
    }

    public ValueTask<IStreamingSttSession> CreateAsync(SttSessionOptions sessionOptions, CancellationToken cancellationToken = default)
    {
        var vadPath = SpeechModelProvisioner.ResolvePath(_options.Vad.ModelPath);
        var vadRunner = new SileroOnnxVadRunner(vadPath);
        var session = new StreamingSttSession(sessionOptions.SessionId, _options, _transcriber, vadRunner);

        return ValueTask.FromResult<IStreamingSttSession>(session);
    }
}
