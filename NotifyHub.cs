using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalRNotification
{
    public class NotifyHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            var userId = Context.ConnectionId;
            await Clients.Caller.SendAsync("onConnected", true, userId);
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.ConnectionId;
            await Clients.Caller.SendAsync("onDisconnected", true, userId, exception);
        }
        public async Task SendMessage(string message="abc")
        {
            await Clients.All.SendAsync("onChanged", message);
        }

        public async Task ReceiveMessage(string message)
        {
            await Clients.All.SendAsync("onChanged", message);
        }

    }
}
