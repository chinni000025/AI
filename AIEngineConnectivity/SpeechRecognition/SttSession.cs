namespace AIEngineConnectivity.SpeechRecognition
{
    public record SttOfferRequest(Guid SessionId, string SdpOffer, string ConnectionId);
    public record SttCandidateRequest(Guid SessionId, string Candidate, string? SdpMid, ushort? SdpMLineIndex);
    public record SttStopRequest(Guid SessionId);
}
