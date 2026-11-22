using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using AIEngineConnectivity.SpeechRecognition;
using AIEngineGateway.Hub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace AIEngineGateway.SpeechRecognition;

public sealed class TranscriptPump
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserSessionManager? _userSessionManager;

    public TranscriptPump(IHubContext<NotificationHub> hubContext, IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _userSessionManager = serviceProvider.GetService<IUserSessionManager>();
    }

    public async Task StartAsync(IStreamingSttSession session, string connectionId, string? userId, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var transcriptEvent in session.ReadTranscriptsAsync(cancellationToken))
            {
                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync(EngineConstants.TranscriptReceived,
                        transcriptEvent, cancellationToken);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    await _hubContext.Clients.User(userId).SendAsync(EngineConstants.TranscriptReceived,
                        transcriptEvent, cancellationToken);

                    if (_userSessionManager != null && !string.IsNullOrEmpty(connectionId))
                    {
                        var connections = _userSessionManager.GetConnections(userId, connectionId);
                        if (connections != null)
                        {
                            foreach (var conn in connections)
                            {
                                if (conn != connectionId)
                                {
                                    await _hubContext.Clients.Client(conn).SendAsync(EngineConstants.TranscriptReceived, transcriptEvent, cancellationToken);
                                }
                            }
                        }
                    }
                }

                // Also publish to All so any active connection for this session receives it
                await _hubContext.Clients.All.SendAsync(EngineConstants.TranscriptReceived, transcriptEvent, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        catch
        {
            throw;
        }
    }
}
