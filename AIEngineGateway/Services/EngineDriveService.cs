using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.Options;
using System.CodeDom;

namespace AIEngineGateway.Services
{
    public class EngineDriveService : IEngineDriveService
    {
        private readonly IUserService _userService;
        private readonly TimeSpan _engineUploadFileTTL;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<EngineDriveService> _logger;

        public EngineDriveService(IUserService userService, IRepositoryWrapper repositoryWrapper,
            IOptions<EngineUploadFileTTL> options, ILogger<EngineDriveService> logger)
        {
            _userService = userService;
            _repositoryWrapper = repositoryWrapper;
            _engineUploadFileTTL = TimeSpan.FromMinutes(options.Value.Expires);
            _logger = logger;
        }

        public async Task<Guid> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var uploadSessionId = Guid.NewGuid();

            EngineFileUploadingSession engineFileUploadingSession = new EngineFileUploadingSession
            {
                Id = uploadSessionId,
                UserId = _userService.GetCurrentUser?.UserId,
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                UploadedBytes = 0,
                UploadStatus = UploadStatus.Initated,
                CreatedAt = now,
                UpdatedAt = now,
                ExpiresAt = now + _engineUploadFileTTL,
            };
            await _repositoryWrapper.GetEngineRepo<EngineFileUploadingSession>().AddAsync(engineFileUploadingSession, cancellationToken);
            await _repositoryWrapper.SaveChangesAsync(cancellationToken);
            return uploadSessionId;
        }

        public async Task UploadChunks(IFormFile formFile, long chunkIndex, Guid sessionId, CancellationToken cancellationToken)
        {

            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream, cancellationToken);
            var chunkBytes = memoryStream.ToArray();
            await _repositoryWrapper.EngineDriveRepository.StoreChunkAtomicAsync(sessionId, chunkIndex,
                chunkBytes, formFile.Length, cancellationToken);
        }

        public async Task FinalizeUploadAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var chunks = await _repositoryWrapper.EngineDriveRepository.GetFileChunksAsync(sessionId, cancellationToken);
            if (chunks is null || !chunks.Any())
            {
                _logger.LogError($"No Chunks found for the sessionId {sessionId}");
                throw new Exception("No chunks found!");
            }
            foreach (var chunk in chunks)
            {

            }
        }
    }
}