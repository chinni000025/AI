using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.DTOs
{
    public record EngineFileResponse(Guid Id, string FileName, long FileSize, EngineFileType EngineFileType,
        string ContentType, DateTime CreatedAt, DateTime ModifiedAt, string? Location, string? parentId);
    public record EngineFileStorageInfo(long ActiveStorage = 0, long TrashStorage = 0);
}
