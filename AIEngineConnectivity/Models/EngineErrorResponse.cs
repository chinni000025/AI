namespace AIEngineConnectivity.Models
{
    public class EngineErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string CorrelationId { get; set; }
    }
}
