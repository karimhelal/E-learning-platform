using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Web.Hubs
{
    public class NotificationHub : Hub
    {
        // Add Admin to group "Admins"
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
    }
}
