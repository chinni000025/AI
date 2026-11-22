using AIEngineConnectivity.SpeechRecognition;
using AIEngineGateway.SpeechRecognition;
using AIEngineGateway.WebRtc;
using SIPSorcery.Net;

namespace AIEngineGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SttController : ControllerBase
{
    private readonly IStreamingSttSessionFactory _sessionFactory;
    private readonly SttSessionRegistry _sessionRegistry;
    private readonly TranscriptPump _transcriptPump;
    private readonly ILogger<SttController> _logger;
    private readonly ILogger<WebRtcAudioIngress> _webRtcLogger;

    public SttController(
        IStreamingSttSessionFactory sessionFactory,
        SttSessionRegistry sessionRegistry,
        TranscriptPump transcriptPump,
        ILogger<SttController> logger,
        ILogger<WebRtcAudioIngress> webRtcLogger)
    {
        _sessionFactory = sessionFactory;
        _sessionRegistry = sessionRegistry;
        _transcriptPump = transcriptPump;
        _logger = logger;
        _webRtcLogger = webRtcLogger;
    }

    [HttpPost("offer")]
    public async Task<IActionResult> HandleOffer([FromBody] SttOfferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sessionOptions = new SttSessionOptions(SessionId: request.SessionId, UserId: request.ConnectionId);
            var session = await _sessionFactory.CreateAsync(sessionOptions, cancellationToken);

            var cts = new CancellationTokenSource();
            var (peerConnection, sdpAnswer) = await WebRtcAudioIngress.CreateReceiverPeerAsync(request.SdpOffer, session, _webRtcLogger, cts.Token);

            var context = new ActiveSttContext(request.SessionId, request.ConnectionId, session, peerConnection, cts);

            _sessionRegistry.Register(context);

            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Start SignalR transcript pump in background for this session
            _ = Task.Run(async () =>
            {
                await _transcriptPump.StartAsync(session, request.ConnectionId, userId, cts.Token);
            }, cts.Token);

            return Ok(new { sdpAnswer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize STT WebRTC session {SessionId}", request.SessionId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("candidate")]
    public IActionResult AddIceCandidate([FromBody] SttCandidateRequest request)
    {
        try
        {
            if (_sessionRegistry.TryGet(request.SessionId, out var context) && context != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Candidate))
                {
                    var init = new RTCIceCandidateInit
                    {
                        candidate = request.Candidate,
                        sdpMid = request.SdpMid,
                        sdpMLineIndex = request.SdpMLineIndex ?? 0
                    };
                    context.PeerConnection.addIceCandidate(init);
                }
                return Ok(new { success = true });
            }

            return Ok(new { success = false, reason = "SessionPendingOrClosed" });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ignored candidate error for session {SessionId}", request.SessionId);
            return Ok(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("stop")]
    public async Task<IActionResult> StopSession([FromBody] SttStopRequest request)
    {
        var removed = await _sessionRegistry.CloseAndRemoveAsync(request.SessionId);
        return Ok(new { stopped = removed });
    }
}
