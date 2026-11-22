using AIEngineConnectivity.SpeechRecognition;
using SIPSorcery.Net;
using System.Collections.Concurrent;

namespace AIEngineGateway.SpeechRecognition;

public record ActiveSttContext(
    Guid SessionId,
    string ConnectionId,
    IStreamingSttSession Session,
    RTCPeerConnection PeerConnection,
    CancellationTokenSource Cts);

public sealed class SttSessionRegistry
{
    private readonly ConcurrentDictionary<Guid, ActiveSttContext> _activeSessions = new();

    public void Register(ActiveSttContext context)
    {
        _activeSessions[context.SessionId] = context;
    }

    public bool TryGet(Guid sessionId, out ActiveSttContext? context)
    {
        return _activeSessions.TryGetValue(sessionId, out context);
    }

    public async Task<bool> CloseAndRemoveAsync(Guid sessionId)
    {
        if (_activeSessions.TryRemove(sessionId, out var context))
        {
            context.Cts.Cancel();
            context.PeerConnection.Close("Session closed by user or timeout");
            await context.Session.CompleteInputAsync();
            await context.Session.DisposeAsync();
            context.Cts.Dispose();
            return true;
        }
        return false;
    }
}
