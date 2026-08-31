using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Entities
{
    public class EngineFileUploadingSession
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string? ParentFolderId { get; set; }
        public string? Location { get; set; }
        public long FileSize { get; set; }
        public long UploadedBytes { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public ICollection<FileChunks> FileChunks { get; set; }
    }
}