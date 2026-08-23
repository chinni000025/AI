using AIEngineConnectivity.EngineCore;
using AIEngineConnectivity.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AIEngineCore.EngineCore
{
    public class EngineQueue<T> : IEngineQueue<T>
    {
        private Channel<T> _priorityChannel;
        private Channel<T> _normalChannel;
        public EngineQueue(int capacity = 10000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = false,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _priorityChannel = Channel.CreateBounded<T>(options);
            _normalChannel = Channel.CreateBounded<T>(options);
        }

        public async ValueTask publishAsync(T scenario, Priority priority = Priority.None, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(scenario);
            if (priority != Priority.None)
            {
                await _priorityChannel.Writer.WriteAsync(scenario, cancellationToken);
            }
            else
            {
                await _normalChannel.Writer.WriteAsync(scenario, cancellationToken);
            }
        }

        public IAsyncEnumerable<T> ReadAsync(CancellationToken cancellationToken = default)
        {
            return ReadAllWithPriorityAsync(cancellationToken);
        }

        private async IAsyncEnumerable<T> ReadAllWithPriorityAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_priorityChannel.Reader.TryRead(out var highItem))
                {
                    yield return highItem;
                    continue;
                }

                if (_normalChannel.Reader.TryRead(out var normalItem))
                {
                    yield return normalItem;
                    continue;
                }

                var highWait = _priorityChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();
                var normalWait = _normalChannel.Reader.WaitToReadAsync(cancellationToken).AsTask();
                await Task.WhenAny(highWait, normalWait);
            }
        }

        public void Complete()
        {
            _priorityChannel.Writer.TryComplete();
            _normalChannel.Writer.TryComplete();
        }
    }
}