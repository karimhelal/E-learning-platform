using DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web.Interfaces;

namespace Web.Controllers.Admin.API
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public NotificationsController(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = _currentUserService.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            List<object> notifs;

            if (isAdmin)
            {
                // For admins: get both user-specific notifications AND admin notifications (UserId == null)
                notifs = await _context.Notifications
                    .AsNoTracking()
                    .Where(n => !n.IsRead && (n.UserId == userId || n.UserId == null))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .Select(n => new { n.Id, n.Title, n.Message, n.Url, n.CreatedAt })
                    .Cast<object>()
                    .ToListAsync();
            }
            else
            {
                // For non-admins: get only user-specific notifications
                notifs = await _context.Notifications
                    .AsNoTracking()
                    .Where(n => !n.IsRead && n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .Select(n => new { n.Id, n.Title, n.Message, n.Url, n.CreatedAt })
                    .Cast<object>()
                    .ToListAsync();
            }

            return Ok(notifs);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _currentUserService.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            List<Core.Entities.Notification> unreadNotifs;

            if (isAdmin)
            {
                // For admins: mark both user-specific AND admin notifications (UserId == null) as read
                unreadNotifs = await _context.Notifications
                    .Where(n => !n.IsRead && (n.UserId == userId || n.UserId == null))
                    .ToListAsync();
            }
            else
            {
                // For non-admins: mark only user-specific notifications as read
                unreadNotifs = await _context.Notifications
                    .Where(n => !n.IsRead && n.UserId == userId)
                    .ToListAsync();
            }

            if (unreadNotifs.Any())
            {
                foreach (var n in unreadNotifs) n.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
