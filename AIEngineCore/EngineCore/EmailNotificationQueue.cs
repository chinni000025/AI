namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System;
    using System.Threading.Channels;

    public class EmailNotificationQueue<T> : IEmailNotificationQueue<T>
    {
        private readonly Channel<T> _channel;
        public EmailNotificationQueue(int capacity = 10000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);
        }
        public async ValueTask PublishEmailNotification(T t, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(t);
            await _channel.Writer.WriteAsync(t, cancellationToken);
        }

        public async ValueTask GetEmailNotification(CancellationToken cancellationToken = default)
        {
            await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}