using BLL.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<PagedResult<AdminUserDto>> GetUsersAsync(string search, string role, int page = 1, int pageSize = 10);
        Task<bool> DeleteUserAsync(int userId);

        // Courses Management
        Task<PagedResult<AdminCourseDto>> GetCoursesAsync(string search, string category, int page = 1, int pageSize = 10);
        Task<bool> DeleteCourseAsync(int courseId);

        Task<List<CategoryDto>> GetAllCategoriesAsync();
    }
}
