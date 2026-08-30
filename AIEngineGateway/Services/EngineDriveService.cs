using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using Microsoft.Extensions.Options;

namespace AIEngineGateway.Services
{
    public class EngineDriveService : IEngineDriveService
    {
        private readonly IUserService _userService;
        private readonly TimeSpan _engineUploadFileTTL;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public EngineDriveService(IUserService userService, IRepositoryWrapper repositoryWrapper,
            IOptions<EngineUploadFileTTL> options)
        {
            _userService = userService;
            _repositoryWrapper = repositoryWrapper;
            _engineUploadFileTTL = TimeSpan.FromMinutes(options.Value.Expires);
        }

        public async Task<long> InitiateFileUpload(UploadInitiateRequest request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var FileId = Guid.NewGuid();
            var ContentId = Guid.NewGuid();
            FileContent fileContent = new FileContent
            {
                Id = ContentId,
            };

            EngineFile engineFile = new EngineFile
            {
                Id = FileId,
                ContentId = ContentId,
                FileName = request.FileName,
                ContentType = request.ContentType,
                ParentId = null, // will address in future
                Location = null, //Will address in future.
                FileSize = request.FileSize,
                IsRecyled = false,
                ItemType = EngineFileType.File,
                CreatedBy = int.Parse(_userService.GetCurrentUser.UserId),
                CreatedAt = now,
                ModifiedBy = now,
            };

            EngineFileUploadingSession engineFileUploadingSession = new EngineFileUploadingSession
            {
                SessionId = request.SessionId,
                FileId = engineFile.Id,
                UserId = _userService.GetCurrentUser?.UserId,
                FileSize = request.FileSize,
                UploadedBytes = 0,
                UploadStatus = UploadStatus.Initated,
                CreatedAt = now,
                UpdatedAt = now,
                ExpiresAt = now + _engineUploadFileTTL,
            };

            await _repositoryWrapper.GetEngineRepo<FileContent>().AddAsync(fileContent, cancellationToken);
            await _repositoryWrapper.GetEngineRepo<EngineFile>().AddAsync(engineFile, cancellationToken);
            await _repositoryWrapper.GetEngineRepo<EngineFileUploadingSession>().AddAsync(engineFileUploadingSession, cancellationToken);
            await _repositoryWrapper.SaveChangesAsync(cancellationToken);
            return engineFileUploadingSession.UploadedBytes;
        }
    }
}