namespace AIEngineConnectivity.SpeechRecognition
{
    public interface IStreamingSttSession : IAsyncDisposable
    {
        Guid SessionId { get; }

        ValueTask PushEncodedAudioPacketAsync(ReadOnlyMemory<byte> payload, AudioPacketMetadata metadata,
            CancellationToken cancellationToken = default);
        IAsyncEnumerable<TranscriptEventDto> ReadTranscriptsAsync(CancellationToken cancellationToken = default);
        ValueTask CompleteInputAsync(CancellationToken cancellationToken = default);
    }
}
