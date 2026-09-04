using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace AIEngineGateway.Repositories
{
    public class EngineDriveRepository : IEngineDriveRepository
    {
        private readonly EngineContext _engineContext;
        private readonly ILogger<EngineDriveRepository> _logger;
        public EngineDriveRepository(EngineContext engineContext, ILogger<EngineDriveRepository> logger)
        {
            _engineContext = engineContext;
            _logger = logger;
        }

        public async Task<List<FileChunks>?> GetFileChunksAsync(Guid sesssionId, CancellationToken cancellationToken)
        {
            var query = await (from f in _engineContext.FileChunks
                               where f.SessionId == sesssionId
                               orderby f.ChunkIndex ascending
                               select f).ToListAsync(cancellationToken);
            return query;
        }

        public async Task StoreChunkAtomicAsync(Guid sessionId, long chunkIndex,
            byte[] chunkBytes, long chunkSize, CancellationToken cancellationToken)
        {
            var strategy = _engineContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                var transaction = await _engineContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var uploadingSession = await (from us in _engineContext.EngineFileUploadingSessions
                                                  where us.Id == sessionId
                                                  select us).FirstOrDefaultAsync(cancellationToken);

                    if (uploadingSession is null)
                    {
                        _logger.LogError($"Uploading session Not found for session Id {sessionId}");
                        throw new Exception($"Uploading session Id is not fount {sessionId}");
                    }

                    if (uploadingSession.UploadStatus != UploadStatus.Initated
                    && uploadingSession.UploadStatus != UploadStatus.Uploading)
                    {
                        throw new Exception($"Upload session {sessionId} is not accepting chunks. Current status: {uploadingSession.UploadStatus}");
                    }

                    if (uploadingSession.ExpiresAt <= DateTime.UtcNow)
                    {
                        _logger.LogError($"Uploading session Id is Expired for Session Id {sessionId}");
                        throw new Exception("uploading session is expired");
                    }
                    var existingChunk = await _engineContext.FileChunks.AsNoTracking()
                                         .FirstOrDefaultAsync(f => f.SessionId == sessionId
                                         && f.ChunkIndex == chunkIndex, cancellationToken);

                    if (existingChunk is null)
                    {
                        //Needs to validate the size instead of blindly proceed.
                        if (uploadingSession.UploadedBytes + chunkSize > uploadingSession.FileSize)
                        {
                            throw new Exception("Chunk exceeds the remaining file size ");
                        }

                        FileChunks fileChunks = new FileChunks
                        {
                            SessionId = sessionId,
                            Chunk = chunkBytes,
                            ChunkIndex = chunkIndex,
                        };

                        await _engineContext.FileChunks.AddAsync(fileChunks, cancellationToken);

                        var effectedRows = await _engineContext.EngineFileUploadingSessions
                            .Where(u => u.Id == sessionId)
                        .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.UploadStatus, UploadStatus.Uploading)
                        .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                        .SetProperty(x => x.UploadedBytes, x => x.UploadedBytes + chunkSize));
                        if (effectedRows != 1)
                        {
                            throw new Exception("Upload session could not be updated.");
                        }
                        await _engineContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            });
        }
    }
}
