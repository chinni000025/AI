# Zero-Memory Chunk Upload Guide (PostgreSQL OID & SQL Server Streaming)

This guide provides the complete, production-ready code changes required to upload chunks with **zero in-memory buffering**.

---

## 1. The Core Architecture

### The Problem in Previous Implementation
```csharp
// ⚠️ Allocates full chunk on Large Object Heap (LOH)
await using var memoryStream = new MemoryStream();
await formFile.CopyToAsync(memoryStream);
var chunkBytes = memoryStream.ToArray(); 
```

### The Solution: End-to-End Stream Pipeline
Instead of converting the HTTP file into a `byte[]`:
1. Open the incoming HTTP stream: `formFile.OpenReadStream()`.
2. Pass the `Stream` down through `EngineDriveService` to `EngineDriveRepository`.
3. **For PostgreSQL**: Use `NpgsqlLargeObjectManager` to stream directly into PostgreSQL's `pg_largeobject` table and obtain a 32-bit `uint` (OID).
4. **For SQL Server**: Use ADO.NET `SqlCommand` with `SqlParameter.Value = chunkStream` (`SqlDbType.VarBinary`, length `-1`), streaming directly into `varbinary(max)` over TDS network packets without EF Core change tracking.

---

## 2. Code Changes by Layer

---

### Layer 1: Interface Definition
**File:** `AIEngineConnectivity/Repositories/IEngineDriveRepository.cs`

Update `StoreChunkAtomicAsync` to accept a `Stream` instead of `byte[] chunkBytes`:

```csharp
using AIEngineConnectivity.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIEngineConnectivity.Repositories
{
    public interface IEngineDriveRepository
    {
        Task<List<FileChunks>?> GetFileChunksAsync(Guid sessionId, CancellationToken cancellationToken);

        Task StoreChunkAtomicAsync(
            Guid sessionId, 
            long chunkIndex, 
            Stream chunkStream, 
            long chunkSize, 
            CancellationToken cancellationToken);
    }
}
```

---

### Layer 2: Service Implementation
**File:** `AIEngineGateway/Services/EngineDriveService.cs`

In `UploadChunks`, eliminate `MemoryStream` and `byte[]`. Open the read stream directly from `IFormFile`:

```csharp
public async Task UploadChunks(IFormFile formFile, long chunkIndex, Guid sessionId, CancellationToken cancellationToken)
{
    // Stream directly from the HTTP request body without buffering into RAM
    await using var chunkStream = formFile.OpenReadStream();

    await _repositoryWrapper.EngineDriveRepository.StoreChunkAtomicAsync(
        sessionId,
        chunkIndex,
        chunkStream,
        formFile.Length,
        cancellationToken);
}
```

---

### Layer 3: Repository Implementation
**File:** `AIEngineGateway/Repositories/EngineDriveRepository.cs`

This is where the database-specific zero-memory logic executes.

#### Required Usings at the top of `EngineDriveRepository.cs`:
```csharp
using System.Data;
using System.IO;
using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Repositories;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
```

#### Updated `StoreChunkAtomicAsync` Method:
```csharp
public async Task StoreChunkAtomicAsync(
    Guid sessionId, 
    long chunkIndex,
    Stream chunkStream, 
    long chunkSize, 
    CancellationToken cancellationToken)
{
    var strategy = _engineContext.Database.CreateExecutionStrategy();
    await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await _engineContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Validate Upload Session
            var uploadingSession = await _engineContext.EngineFileUploadingSessions
                .FirstOrDefaultAsync(us => us.Id == sessionId, cancellationToken);

            if (uploadingSession is null)
            {
                _logger.LogError("Uploading session not found for session Id {SessionId}", sessionId);
                throw new Exception($"Uploading session Id not found: {sessionId}");
            }

            if (uploadingSession.UploadStatus != UploadStatus.Initated
                && uploadingSession.UploadStatus != UploadStatus.Uploading)
            {
                throw new Exception($"Upload session {sessionId} is not accepting chunks. Current status: {uploadingSession.UploadStatus}");
            }

            if (uploadingSession.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogError("Uploading session expired for Session Id {SessionId}", sessionId);
                throw new Exception("Uploading session is expired.");
            }

            // 2. Validate Size Bounds
            if (uploadingSession.UploadedBytes + chunkSize > uploadingSession.FileSize)
            {
                throw new Exception("Chunk exceeds the remaining file size.");
            }

            // 3. Deduplication: Check if chunk index already exists
            var chunkExists = await _engineContext.FileChunks.AsNoTracking()
                .AnyAsync(f => f.SessionId == sessionId && f.ChunkIndex == chunkIndex, cancellationToken);

            if (chunkExists)
            {
                _logger.LogWarning("Chunk index {ChunkIndex} already uploaded for session {SessionId}. Skipping.", chunkIndex, sessionId);
                return;
            }

            // 4. Database-Specific Zero-Memory Streaming
            if (_engineContext.Database.IsNpgsql())
            {
                // ==========================================
                // POSTGRESQL: Large Object (OID) Streaming
                // ==========================================
                var connection = (NpgsqlConnection)_engineContext.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                var manager = new NpgsqlLargeObjectManager(connection);

                // Create Large Object (returns uint OID)
                uint oid = await manager.CreateAsync(0, cancellationToken);

                // Stream directly into pg_largeobject
                await using (var loStream = await manager.OpenReadWriteAsync(oid, cancellationToken))
                {
                    await chunkStream.CopyToAsync(loStream, cancellationToken);
                }

                // Insert metadata entity in EF Core
                var fileChunk = new FileChunks
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    ChunkIndex = chunkIndex,
                    ChunkOid = oid,
                    ChunkData = null
                };

                await _engineContext.FileChunks.AddAsync(fileChunk, cancellationToken);
                await _engineContext.SaveChangesAsync(cancellationToken);
            }
            else if (_engineContext.Database.IsSqlServer())
            {
                // ==========================================
                // SQL SERVER: Direct ADO.NET Streaming
                // ==========================================
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

                // Stream parameter: ADO.NET streams this directly to varbinary(max)
                var dataParam = new SqlParameter("@ChunkData", SqlDbType.VarBinary, -1)
                {
                    Value = chunkStream
                };
                cmd.Parameters.Add(dataParam);

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"Unsupported database provider: {_engineContext.Database.ProviderName}");
            }

            // 5. Atomic Update of Uploading Session Status & Progress
            var affectedRows = await _engineContext.EngineFileUploadingSessions
                .Where(u => u.Id == sessionId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.UploadStatus, UploadStatus.Uploading)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow)
                    .SetProperty(x => x.UploadedBytes, x => x.UploadedBytes + chunkSize),
                    cancellationToken);

            if (affectedRows != 1)
            {
                throw new Exception("Upload session progress could not be updated.");
            }

            // 6. Commit Transaction
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    });
}
```

---

## 3. How Memory Behaves Under the Hood

| Component | What it does | Memory Impact |
| :--- | :--- | :--- |
| `formFile.OpenReadStream()` | Reads incoming HTTP multipart body incrementally | Small 4KB–8KB buffer |
| **Postgres** `NpgsqlLargeObjectStream` | Writes directly to PostgreSQL server in 2KB pages | **~0 MB RAM allocated in C#** |
| **SQL Server** `SqlParameter(Stream)` | TDS protocol pipes bytes straight into network sockets | **~0 MB RAM allocated in C#** |
| `GC / LOH` | No `byte[]` > 85KB is ever created | **Zero LOH allocations, Zero GC pauses** |
