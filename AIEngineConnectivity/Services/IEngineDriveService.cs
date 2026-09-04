using AIEngineConnectivity.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineDriveService
    {
        public Task<Guid> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken);
        public Task UploadChunks(IFormFile formFile, long chunkIndex, Guid sessionId, CancellationToken cancellationToken);
        public Task FinalizeUploadAsync(Guid sessionId, CancellationToken cancellationToken);
    }
}
