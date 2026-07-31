namespace AIEngineCore.EngineCore
{
    public class EngineEventPublisher<T>
    {
        private EngineQueue<T> _EngineQueue;

        public EngineEventPublisher()
        {
            _EngineQueue = new EngineQueue<T>();
        }
        public async Task PublishEvent(T e, CancellationToken cancellationToken = default)
        {
            await _EngineQueue.publishAsync(e, cancellationToken);
        }
    }
}