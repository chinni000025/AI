namespace AIEngineConnectivity.EngineCore
{
    public interface IEmailNotificationQueue<T>
    {
        public ValueTask PublishEmailNotification(T t, CancellationToken cancellationToken = default);
        public ValueTask GetEmailNotification(CancellationToken cancellationToken = default);
    }
}