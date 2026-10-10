namespace AIEngineConnectivity.Models
{
    public class DownloadFileResult : IAsyncDisposable
    {
        private Func<Task> _cleanUp;
        private int _disposed;

        public required Stream Stream { get; set; }
        public required string ContentType { get; set; }
        public required string FileName { get; set; }
        public required long FileSize { get; set; }
        public required long RangeStart { get; init; }
        public required long BytesToSend { get; init; }

        public required Func<Task> CleanUpAsync
        {
            init => _cleanUp = value;
        }
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }
            try
            {
                await Stream.DisposeAsync();
            }
            finally
            {
                await _cleanUp();
            }
        }
    }
}
