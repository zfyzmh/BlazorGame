using Microsoft.AspNetCore.SignalR;

namespace Gobang.GameHub
{
    public class GoBangHub : Hub
    {
        public const string HubUrl = "/gobang";

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Disconnected {exception?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}