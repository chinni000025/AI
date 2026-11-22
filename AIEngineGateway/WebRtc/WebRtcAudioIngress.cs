using AIEngineConnectivity.Constants;
using AIEngineConnectivity.SpeechRecognition;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace AIEngineGateway.WebRtc;

public sealed class WebRtcAudioIngress
{
    public static async Task<(RTCPeerConnection peerConnection, string sdpAnswer)> CreateReceiverPeerAsync(string sdpOffer,
        IStreamingSttSession session, ILogger<WebRtcAudioIngress> logger, CancellationToken cancellationToken = default)
    {
        var config = new RTCConfiguration
        {
            iceServers = new List<RTCIceServer>
            {
                new RTCIceServer { urls = WebRTC.STUNServer }
            }
        };

        var peerConnection = new RTCPeerConnection(config);

        // Add audio track for Opus 48kHz RecvOnly
        var audioFormat = new AudioFormat(111, "opus", 48000, 2);
        var audioTrack = new MediaStreamTrack(audioFormat, MediaStreamStatusEnum.RecvOnly);
        peerConnection.addTrack(audioTrack);

        long packetCount = 0;
        // Forward RTP audio packets directly to speech recognition session
        peerConnection.OnRtpPacketReceived += (remoteEP, mediaType, rtpPacket) =>
        {
            if (mediaType == SDPMediaTypesEnum.audio)
            {
                packetCount++;

                var metadata = new AudioPacketMetadata(
                    rtpPacket.Header.PayloadType,
                    rtpPacket.Header.Timestamp,
                    rtpPacket.Header.MarkerBit == 1,
                    DateTimeOffset.UtcNow);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await session.PushEncodedAudioPacketAsync(rtpPacket.Payload, metadata, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }, cancellationToken);
            }
        };

        var offerInit = new RTCSessionDescriptionInit
        {
            type = RTCSdpType.offer,
            sdp = sdpOffer
        };

        var result = peerConnection.setRemoteDescription(offerInit);
        if (result != SetDescriptionResultEnum.OK)
        {
            logger.LogError($"Failed to set remote WebRTC description: {result}");
            throw new InvalidOperationException();
        }

        var answer = peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);

        // Wait briefly for local ICE candidates to gather so the answer contains them
        if (peerConnection.iceGatheringState != RTCIceGatheringState.complete)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnGatheringChange(RTCIceGatheringState state)
            {
                if (state == RTCIceGatheringState.complete)
                {
                    tcs.TrySetResult(true);
                }
            }
            peerConnection.onicegatheringstatechange += OnGatheringChange;
            try
            {
                await Task.WhenAny(tcs.Task, Task.Delay(800, cancellationToken));
            }
            finally
            {
                peerConnection.onicegatheringstatechange -= OnGatheringChange;
            }
        }

        var finalSdp = peerConnection.localDescription?.sdp?.ToString() ?? answer.sdp;
        return (peerConnection, finalSdp);
    }
}
