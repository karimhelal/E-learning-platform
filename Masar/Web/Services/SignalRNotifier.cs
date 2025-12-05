using BLL.Interfaces.Admin;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;
using Web.Interfaces;

namespace Web.Services
{
    public class SignalRNotifier : INotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToAdminsAsync(string title, string message, string url)
        {
            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", title, message, url);
        }

        public async Task SendToUserAsync(int userId, string title, string message, string url)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", title, message, url);
        }
    }
}
