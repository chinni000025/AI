using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Data;

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
                                         && f.ChunkIndex == chunkIndex, cancellationToken);

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
    }
}
