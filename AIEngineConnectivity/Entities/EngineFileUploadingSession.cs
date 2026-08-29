using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Entities
{
    public class EngineFileUploadingSession
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public long FileSize { get; set; }
        public long UploadedBytes { get; set; }
        public UploadStatus UploadStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}