using AIEngineConnectivity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AIEngineGateway.Hub
{
#nullable disable
    /// <summary>
    /// It needs Authentication.
    /// </summary>
    [Authorize]
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IUserSessionManager _userSessionManager;
        public NotificationHub(IUserSessionManager userSessionManager)
        {
            _userSessionManager = userSessionManager;
        }

        public override async Task OnConnectedAsync()
        {

            var userId = Context.UserIdentifier;
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext.Request.Query["sessionId"].ToString();
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
                _userSessionManager.AddConnection(userId, sessionId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            var userId = Context.UserIdentifier;
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext.Request.Query["sessionId"].ToString();
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
                _userSessionManager.RemoveConnection(userId, sessionId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
