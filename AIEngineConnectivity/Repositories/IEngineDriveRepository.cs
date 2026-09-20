using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Services;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineDriveRepository
    {
        public Task<List<FileChunks>?> GetFileChunksAsync(Guid sesssionId, CancellationToken cancellationToken);
        public Task StoreChunkAtomicAsync(Guid sessionId, long chunkIndex, Stream chunkStream, long chunkSize,
            CancellationToken cancellationToken);
        public Task FinalizeUploadAtomicAsync(Guid sessionId, IUserService userService, CancellationToken cancellationToken);
        public Task<List<EngineFileResponse>> GetEngineFilesAsync(int userId, CancellationToken cancellationToken);
        public Task StaleEngineUploadingSessionsAndChunks(CancellationToken cancellationToken);
        public Task<EngineFileStorageInfo> GetEngineStorageInfo(int userId, CancellationToken cancellationToken);
        public Task DeleteEngineFileAsync(Guid Id, CancellationToken cancellationToken);
    }
}