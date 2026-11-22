using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#nullable disable
namespace AIEngineGateway.Services
{
    public class EngineDriveService : IEngineDriveService
    {
        private readonly IUserService _userService;
        private readonly TimeSpan _engineUploadFileTTL;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger<EngineDriveService> _logger;
        private readonly IEngineScheduler _engineScheduler;

        public EngineDriveService(IUserService userService, IRepositoryWrapper repositoryWrapper,
            IOptions<EngineUploadFileTTL> options, ILogger<EngineDriveService> logger,
            IEngineScheduler engineScheduler)
        {
            _userService = userService;
            _repositoryWrapper = repositoryWrapper;
            _engineUploadFileTTL = TimeSpan.FromMinutes(options.Value.Expires);
            _logger = logger;
            _engineScheduler = engineScheduler;
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
                UploadStatus = UploadStatus.Initiated,
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
            await using var chunkStream = formFile.OpenReadStream();
            await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
            {
                await _repositoryWrapper.EngineDriveRepository
                        .StoreChunkAtomicAsync(sessionId, chunkIndex,
                            chunkStream, formFile.Length, cancellationToken);
            }, cancellationToken);
        }

        public async Task FinalizeUploadAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            try
            {
                await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
                {
                    await _repositoryWrapper.EngineDriveRepository.FinalizeUploadAtomicAsync(sessionId, _userService, cancellationToken);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception("Error Occured while saving the file");
            }
        }

        public async Task<List<EngineFileResponse>> GetAvailableFilesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _repositoryWrapper.EngineDriveRepository
                    .GetEngineFilesAsync(int.Parse(_userService?.GetCurrentUser?.UserId), false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<EngineFileResponse>> GetTrashFilesAsync(CancellationToken cancellationToken)
        {
            return await _repositoryWrapper.EngineDriveRepository
                .GetEngineFilesAsync(int.Parse(_userService?.GetCurrentUser?.UserId), true, cancellationToken);
        }

        public async Task<EngineFileStorageInfo> GetEngineStorageInfo(CancellationToken cancellationToken)
        {
            try
            {
                return await _repositoryWrapper.EngineDriveRepository
                    .GetEngineStorageInfo(int.Parse(_userService.GetCurrentUser?.UserId), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occur while getting Storage Info :: " + ex);
                throw;
            }
        }

        public async Task TrashFileByIds(List<Guid> ids, CancellationToken cancellationToken)
        {
            await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
            {
                var now = DateTime.Now;
                var userId = _userService?.GetCurrentUser?.UserId;

                var recycleEntities = ids.Select(id => new RecycleBin
                {
                    Id = Guid.NewGuid(),
                    EntityId = id.ToString(),
                    ItemId = RecycleItem.EngineFile,
                    CreatedAt = now,
                    ModifiedAt = now,
                    DeletedBy = userId
                }).ToList();

                await _repositoryWrapper.GetEngineRepo<RecycleBin>()
                    .AddRangeAsync(recycleEntities, cancellationToken);
                var files = await _repositoryWrapper.GetEngineRepo<EngineFile>()
                    .UpdatePropertyByIdsAsync(ids, "Id", f => f.IsRecyled, true, cancellationToken);
                await _repositoryWrapper.SaveChangesAsync(cancellationToken);

                foreach (var entity in recycleEntities)
                {
                    await _engineScheduler.DeleteEngineRecycleItems(entity, cancellationToken);
                }
            }, cancellationToken);
        }

        public async Task<EngineFile?> GetDownloadMetadataAsync(Guid FileId, CancellationToken cancellationToken)
        {
            try
            {
                if (!int.TryParse(_userService?.GetCurrentUser?.UserId, out var userId))
                {
                    _logger.LogError("Error while Paring User Id ");
                    throw new FormatException("Error occured while parse the UserId");
                }

                return await _repositoryWrapper.GetEngineRepo<EngineFile>()
                    .FirstOrDefaultNoTrackingAsync(f => f.Id == FileId &&
                        f.CreatedBy == userId && !f.IsRecyled, cancellationToken);
            }
            catch
            {
                throw;
            }
        }

        public async Task<DownloadFileResult?> GetDownloadStreamAsync(Guid fileId, long offset, long? bytesToSend, EngineFile metadata, CancellationToken cancellationToken)
        {
            try
            {
                if (!int.TryParse(_userService?.GetCurrentUser?.UserId, out var userId))
                {
                    _logger.LogError("Error while Paring User Id ");
                    throw new FormatException("Error occured while parse the UserId");
                }
                var fileMetaData = await _repositoryWrapper.GetEngineRepo<EngineFile>().
                    FirstOrDefaultNoTrackingAsync(f => f.Id == fileId && f.CreatedBy == userId, cancellationToken);
                if (fileMetaData is null)
                {
                    _logger.LogError($"File Metadata is not exists with File Id {fileId}");
                    throw new Exception($"File Metadata is not exists with File Id {fileId}");
                }

                return await _repositoryWrapper.EngineDriveRepository
                    .GetDownloadStreamAsync(fileId, offset, bytesToSend, fileMetaData, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred While Get Download Stream Async :: {ex}");
                throw;
            }
        }

        public async Task DeleteFileAsync(List<Guid> fileIds, CancellationToken cancellation)
        {
            try
            {
                foreach (var file in fileIds)
                {
                    await _repositoryWrapper.ExecuteInTransactionAsync(async () =>
                    {
                        await _repositoryWrapper.EngineDriveRepository.DeleteEngineFileAsync(file, cancellation);
                    });
                }
            }
            catch
            {
                throw;
            }
        }
    }
}