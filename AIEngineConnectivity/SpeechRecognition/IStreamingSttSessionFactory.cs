namespace AIEngineConnectivity.SpeechRecognition
{
    public interface IStreamingSttSessionFactory
    {
        ValueTask<IStreamingSttSession> CreateAsync(
            SttSessionOptions options,
            CancellationToken cancellationToken = default);
    }
}
