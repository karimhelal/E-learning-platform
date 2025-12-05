using BLL.DTOs.Admin;

namespace BLL.Interfaces.Admin;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(string search, string role, int page = 1, int pageSize = 10);
    Task<bool> DeleteUserAsync(int userId);

    // Courses Management
    Task<PagedResult<AdminCourseDto>> GetCoursesAsync(string search, string category, int page = 1, int pageSize = 10);
    Task<bool> DeleteCourseAsync(int courseId);

    Task<List<CategoryDto>> GetAllCategoriesAsync();

        // Pending Courses
        Task<List<AdminPendingCourseDto>> GetPendingCoursesAsync();
        Task<int> GetPendingCoursesCountAsync();
        Task ApproveCourseAsync(int courseId);
        Task RejectCourseAsync(int courseId,string reason);
    }
}
