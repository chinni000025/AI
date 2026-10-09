namespace AIEngineConnectivity.Models
{
    public class DownloadFileResult
    {
        public required Stream Stream { get; set; }
        public required string ContentType { get; set; }
        public required string FileName { get; set; }
        public required long FileSize { get; set; }
    }
}
