using AIEngineConnectivity.DTOs;
using Microsoft.AspNetCore.Http;

namespace AIEngineConnectivity.Services
{
    public interface IEngineDriveService
    {
        public Task<Guid> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken);
        public Task UploadChunks(IFormFile formFile, long chunkIndex, Guid sessionId, CancellationToken cancellationToken);
        public Task FinalizeUploadAsync(Guid sessionId, CancellationToken cancellationToken);
        public Task<List<EngineFileResponse>> GetAvailableFilesAsync(CancellationToken cancellationToken);
    }
}
