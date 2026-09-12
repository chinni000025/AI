using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using AIEngineGateway.EngineInfrastructure;
using Google.GenAI.Types;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace AIEngineGateway.Repositories
{
    public class EngineDriveRepository : IEngineDriveRepository
    {
        private readonly EngineContext _engineContext;
        private readonly ILogger<EngineDriveRepository> _logger;
        private readonly TimeSpan _engineUploadFileTTL;
        public EngineDriveRepository(EngineContext engineContext, ILogger<EngineDriveRepository> logger, 
            IOptions<EngineUploadFileTTL> options)
        {
            _engineContext = engineContext;
            _logger = logger;
            _engineUploadFileTTL = TimeSpan.FromMinutes(options.Value.Expires);
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
            Stream chunkStream, long chunkSize, CancellationToken cancellationToken)
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
                                         && f.ChunkIndex == chunkIndex , cancellationToken);

                    if (existingChunk is null)
                    {
                        //Needs to validate the size instead of blindly proceed.
                        if (uploadingSession.UploadedBytes + chunkSize > uploadingSession.FileSize)
                        {
                            throw new Exception("Chunk exceeds the remaining file size ");
                        }

                        if (_engineContext.Database.IsNpgsql())
                        {
                            await PostgresOidStream(sessionId, chunkIndex, chunkStream, cancellationToken);
                        }
                        else if (_engineContext.Database.IsSqlServer())
                        {
                            await SqlServerStream(sessionId, chunkIndex, chunkStream, transaction, cancellationToken);
                        }

                        var effectedRows = await _engineContext.EngineFileUploadingSessions
                            .Where(u => u.Id == sessionId )
                            .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.UploadStatus, UploadStatus.Uploading)
                            .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                            .SetProperty(x=>x.ExpiresAt, DateTime.UtcNow +_engineUploadFileTTL)
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

        private async Task PostgresOidStream(Guid sessionId, long chunkIndex, Stream chunkStream, CancellationToken cancellationToken)
        {
            var connection = (NpgsqlConnection)_engineContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }
            var manager = new NpgsqlLargeObjectManager(connection);
            uint oid = await manager.CreateAsync(0, cancellationToken);
            await using (var loStream = await manager.OpenReadWriteAsync(oid, cancellationToken))
            {
                await chunkStream.CopyToAsync(loStream, cancellationToken);
            }

            FileChunks fileChunks = new FileChunks
            {
                SessionId = sessionId,
                ChunkIndex = chunkIndex,
                ChunkOid = oid,
                ChunkData = null
            };
            await _engineContext.FileChunks.AddAsync(fileChunks, cancellationToken);
            await _engineContext.SaveChangesAsync(cancellationToken);
        }

        private async Task SqlServerStream(Guid sessionId, long chunkIndex, Stream chunkStream, IDbContextTransaction transaction,
            CancellationToken cancellationToken)
        {
            var connection = (SqlConnection)_engineContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbTransaction = (SqlTransaction)transaction.GetDbTransaction();
            var chunkId = Guid.NewGuid();
            const string sql = @"
                    INSERT INTO [FileChunks] ([Id], [SessionId], [ChunkIndex], [ChunkOid], [ChunkData])
                    VALUES (@Id, @SessionId, @ChunkIndex, NULL, @ChunkData);";

            await using var cmd = new SqlCommand(sql, connection, dbTransaction);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = chunkId });
            cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier) { Value = sessionId });
            cmd.Parameters.Add(new SqlParameter("@ChunkIndex", SqlDbType.BigInt) { Value = chunkIndex });
            var dataParam = new SqlParameter("@ChunkData", SqlDbType.VarBinary, -1)
            {
                Value = chunkStream
            };
            cmd.Parameters.Add(dataParam);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task FinalizeUploadAtomicAsync(Guid sessionId,IUserService userService ,CancellationToken cancellationToken)
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
                        _logger.LogError($"Upload session {sessionId} not found");
                        throw new Exception($"Upload session {sessionId} not found");
                    }

                    if (uploadingSession.UploadStatus != UploadStatus.Initated && uploadingSession.UploadStatus != UploadStatus.Uploading)
                    {
                        _logger.LogError($"Session is in an invalid state for finalizing: {uploadingSession.UploadStatus}");
                        throw new Exception($"Session is in an invalid state for finalizing: {uploadingSession.UploadStatus}");
                    }
                    if (uploadingSession.ExpiresAt <= DateTime.UtcNow)
                    {
                        _logger.LogError($"Uploading session is Expired with Session Id {sessionId}");
                        throw new Exception($"Uploading session is Expired with Session Id {sessionId}");
                    }

                    if (uploadingSession.FileSize != uploadingSession.UploadedBytes)
                    {
                        _logger.LogError($"All the Chunks are not getting properly with session Id{sessionId}");
                        throw new Exception($"All the Chunks are not uploaded successfully ");
                    }
                    var chunks = await (from f in _engineContext.FileChunks
                                        where f.SessionId == sessionId
                                        orderby f.ChunkIndex ascending
                                        select f).ToListAsync(cancellationToken);
                    var fileContentId = Guid.NewGuid();
                    if (_engineContext.Database.IsNpgsql())
                    {
                        await PostgresFinalizeOidAsync(fileContentId, chunks, cancellationToken);
                    }
                    else if (_engineContext.Database.IsSqlServer())
                    {
                        await sqlServerFinalizeStreamAsync(fileContentId, sessionId, transaction, cancellationToken);
                    }
                    else
                    {
                        throw new Exception("Internal Server Error");
                    }
                    var userId = int.Parse(userService?.GetCurrentUser.UserId);
                    var now = DateTime.UtcNow;

                    EngineFile engineFile = new EngineFile
                    {

                        ContentId = fileContentId,
                        FileName = uploadingSession.FileName,
                        ContentType = uploadingSession.ContentType,
                        ParentId = null, // Will change later.
                        Location = null, // Will enhance later.
                        FileSize = uploadingSession.FileSize,
                        IsRecyled = false,
                        ItemType = EngineFileType.File,// Further modification needed.
                        CreatedBy = userId,
                        CreatedAt = now,
                        ModifiedBy = now
                    };

                    FileAccessors fileAccessors = new FileAccessors
                    {
                        EngineFile = engineFile,
                        UserId = userId,
                    };
                    await _engineContext.EngineFiles.AddAsync(engineFile, cancellationToken);
                    await _engineContext.FileAccessors.AddAsync(fileAccessors, cancellationToken);
                    await _engineContext.FileChunks.Where(c => c.SessionId == sessionId).ExecuteDeleteAsync(cancellationToken);
                    uploadingSession.UploadStatus = UploadStatus.Completed;
                    uploadingSession.UpdatedAt = now;
                    await _engineContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            });
        }

        private async Task PostgresFinalizeOidAsync(Guid fileContentId,List<FileChunks> chunks,CancellationToken cancellationToken)
        {
            var connection = (NpgsqlConnection)_engineContext.Database.GetDbConnection();
            if(connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var manager = new NpgsqlLargeObjectManager(connection);
            uint finalOid = await manager.CreateAsync(0, cancellationToken);
            await using(var finalStream = await manager.OpenReadWriteAsync(finalOid, cancellationToken))
            {
                foreach (var chunk in chunks)
                {
                    if (chunk.ChunkOid is null)
                    {
                        throw new InvalidOperationException($"Chunk index {chunk.ChunkIndex} is missing OID.");
                    }
                    await using (var chunkStream = await manager.OpenReadAsync(finalOid, cancellationToken))
                    {
                        await chunkStream.CopyToAsync(finalStream, cancellationToken);
                    }

                    // CRITICAL: Unlink (delete) temporary chunk OID from pg_largeobject to prevent orphan bloat!
                    await manager.UnlinkAsync(chunk.ChunkOid.Value, cancellationToken);
                }
            }

            var fileContent = new FileContent
            {
                Id = fileContentId,
                ContentOid = finalOid,
                ContentData = null
            };
            await _engineContext.FileContents.AddAsync(fileContent, cancellationToken);
            await _engineContext.SaveChangesAsync(cancellationToken);
        }

        private async Task sqlServerFinalizeStreamAsync(Guid fileContentId,Guid sessionId,IDbContextTransaction transaction,
            CancellationToken cancellationToken)
        {
            var connection = (SqlConnection)_engineContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var dbTransaction = (SqlTransaction)transaction.GetDbTransaction();
            const string sql = @"
                    INSERT INTO [FileContents] ([Id], [ContentOid], [ContentData])
                    VALUES (@ContentId, NULL, 0x);
                    DECLARE @chunk VARBINARY(MAX);
                    DECLARE chunk_cursor CURSOR LOCAL FAST_FORWARD FOR
                        SELECT [ChunkData]
                        FROM [FileChunks]
                        WHERE [SessionId] = @SessionId
                        ORDER BY [ChunkIndex] ASC;
                    OPEN chunk_cursor;
                    FETCH NEXT FROM chunk_cursor INTO @chunk;
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        UPDATE [FileContents]
                        SET [ContentData].WRITE(@chunk, NULL, 0)
                        WHERE [Id] = @ContentId;
                        FETCH NEXT FROM chunk_cursor INTO @chunk;
                    END
                    CLOSE chunk_cursor;
                    DEALLOCATE chunk_cursor;";
            await using var cmd = new SqlCommand(sql, connection, dbTransaction);
            cmd.Parameters.Add(new SqlParameter("@ContentId", SqlDbType.UniqueIdentifier) { Value = fileContentId });
            cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.UniqueIdentifier) { Value = sessionId });
            // Set timeout higher if finalizing large files (e.g. 5 minutes)
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
