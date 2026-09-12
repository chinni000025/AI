using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineDriveRepository
    {
        public Task<List<FileChunks>?> GetFileChunksAsync(Guid sesssionId, CancellationToken cancellationToken);
        public Task StoreChunkAtomicAsync(Guid sessionId, long chunkIndex, Stream chunkStream, long chunkSize,
            CancellationToken cancellationToken);
        public Task FinalizeUploadAtomicAsync(Guid sessionId, IUserService userService, CancellationToken cancellationToken);
    }
}
