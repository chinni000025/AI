namespace AIEngineConnectivity.SpeechRecognition
{
    public sealed record SttSessionOptions(
      Guid SessionId,
      string UserId,
      int SampleRate = 16000,
      int Channels = 1,
      string? Language = "en");
}
