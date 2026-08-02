namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.EngineCore;
    using System.Threading.Channels;

    public class EngineQueue<T> : IEngineQueue<T>
    {
        private readonly Channel<T> _channel;
        public EngineQueue(int capacity = 10000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);
        }

        public async ValueTask publishAsync(T scenario, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scenario);
            await _channel.Writer.WriteAsync(scenario, cancellationToken);
        }

        public IAsyncEnumerable<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        public void Complete()
        {
            _channel.Writer.TryComplete();
        }
    }
}