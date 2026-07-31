namespace AIEngineConnectivity.EngineCore
{
    using System.Collections.Generic;

    public interface IEngineQueue<T>
    {
        public ValueTask publishAsync(T scenario, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<T> ReadAsync(CancellationToken cancellationToken = default);
    }
}