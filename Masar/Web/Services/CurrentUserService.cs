using System.Security.Claims;
using Web.Interfaces;

namespace Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return 0;
        }


        public int GetStudentId()
        {
            var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("StudentId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        public int GetInstructorId()
        {
            var idStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue("InstructorId");
            return int.TryParse(idStr, out int id) ? id : 0;
        }
    }
}
