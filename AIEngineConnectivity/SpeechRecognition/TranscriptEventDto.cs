namespace AIEngineConnectivity.SpeechRecognition
{
    public sealed record TranscriptEventDto(Guid SessionId, long UtteranceId, long Revision,
              string Text, bool IsFinal, DateTimeOffset CreatedAt, string? Language = null);
}
