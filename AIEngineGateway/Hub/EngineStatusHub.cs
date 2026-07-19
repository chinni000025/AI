namespace AIEngineGateway.Hub
{
    using Microsoft.AspNetCore.SignalR;
    public class EngineStatusHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
