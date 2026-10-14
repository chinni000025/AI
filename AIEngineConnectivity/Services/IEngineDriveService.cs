using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using Microsoft.AspNetCore.Http;

namespace AIEngineConnectivity.Services
{
    public interface IEngineDriveService
    {
        public Task<Guid> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken);
        public Task UploadChunks(IFormFile formFile, long chunkIndex, Guid sessionId, CancellationToken cancellationToken);
        public Task FinalizeUploadAsync(Guid sessionId, CancellationToken cancellationToken);
        public Task<List<EngineFileResponse>> GetAvailableFilesAsync(CancellationToken cancellationToken);
        public Task<List<EngineFileResponse>> GetTrashFilesAsync(CancellationToken cancellationToken);
        public Task<EngineFileStorageInfo> GetEngineStorageInfo(CancellationToken cancellationToken);
        public Task TrashFileByIds(List<Guid> ids, CancellationToken cancellationToken);
        public Task<EngineFile?> GetDownloadMetadataAsync(Guid FileId, CancellationToken cancellationToken);
        public Task<DownloadFileResult?> GetDownloadStreamAsync(Guid fileId, long offset,
            long? bytesToSend, EngineFile metadata, CancellationToken cancellationToken);
        public Task DeleteFileAsync(List<Guid> fileId, CancellationToken cancellation);
    }
}
