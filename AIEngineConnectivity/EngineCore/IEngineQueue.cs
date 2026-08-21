using System.Collections.Generic;

namespace AIEngineConnectivity.EngineCore
{
    public interface IEngineQueue<T>
    {
        public ValueTask publishAsync(T scenario, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<T> ReadAsync(CancellationToken cancellationToken = default);
        public void Complete();
    }
}