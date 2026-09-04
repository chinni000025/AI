using AIEngineConnectivity.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineDriveRepository
    {
        public Task<List<FileChunks>?> GetFileChunksAsync(Guid sesssionId, CancellationToken cancellationToken);
        public Task StoreChunkAtomicAsync(Guid sessionId, long chunkIndex, byte[] chunkBytes, long chunkSize,
            CancellationToken cancellationToken);
    }
}
