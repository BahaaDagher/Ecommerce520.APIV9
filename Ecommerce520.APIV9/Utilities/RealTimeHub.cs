using Microsoft.AspNetCore.SignalR;

namespace Ecommerce520.APIV9.Utilities
{
    public class RealTimeHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
