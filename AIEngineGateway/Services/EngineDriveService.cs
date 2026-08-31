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

            //EngineFileUploadingSession engineFileUploadingSession = new EngineFileUploadingSession
            //{
            //    Id = uploadSessionId,
            //    SessionId = request.SessionId,
            //    FileName = request.FileName,
            //    FileInfo = request.ContentType,
            //    UserId = _userService.GetCurrentUser?.UserId,
            //    FileSize = request.FileSize,
            //    UploadedBytes = 0,
            //    UploadStatus = UploadStatus.Initated,
            //    CreatedAt = now,
            //    UpdatedAt = now,
            //    ExpiresAt = now + _engineUploadFileTTL,
            //};
            //await _repositoryWrapper.GetEngineRepo<EngineFileUploadingSession>().AddAsync(engineFileUploadingSession, cancellationToken);
            //await _repositoryWrapper.SaveChangesAsync(cancellationToken);
            //return uploadSessionId;
            return Guid.NewGuid();
        }

        public async Task UploadChunks(IFormFile formFile, Guid sessionId, CancellationToken cancellationToken)
        {
            var uploadingSession = await _repositoryWrapper.GetEngineRepo<EngineFileUploadingSession>().GetByIdAsync(sessionId, cancellationToken);
            if (uploadingSession is null)
            {
                _logger.LogError($"No uploding session found  for Session Id  {sessionId}");
                throw new Exception("No Uploading Session Found ");
            }

            uploadingSession.UploadedBytes += formFile.Length;
            uploadingSession.UpdatedAt = DateTime.UtcNow;
            await _repositoryWrapper.SaveChangesAsync(cancellationToken);
        }
    }
}