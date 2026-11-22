namespace AIEngineConnectivity.SpeechRecognition
{

    public sealed record AudioPacketMetadata(int PayloadType, uint? RtpTimestamp, bool Marker,
                                                DateTimeOffset ReceivedAt);
}
