using BLL.DTOs.Admin;
using BLL.Interfaces.Admin;
using Core.Entities;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BLL.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly static int PageSize = 10;

        public AdminService(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories.AsNoTracking()
                                 .Select(c => new CategoryDto
                                 {
                                     Id = c.CategoryId,
                                     Name = c.Name
                                 }).ToListAsync();
        }



        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }



        public async Task<PagedResult<AdminCourseDto>> GetCoursesAsync(string search, string category, int page = 1, int pageSize = 10)
        {
            var query = _context.Courses
                                        .AsNoTracking()
                                        .Include(c => c.Instructor)
                                                 .ThenInclude(i => i.User)
                                        .Include(c=>c.Categories)
                                        .Include(c => c.Enrollments).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                            c.Title.Contains(search) ||
                            (c.Description != null && c.Description.Contains(search))
                            || c.Instructor != null && c.Instructor.User != null &&(c.Instructor.User.FirstName.Contains(search) ||
                               c.Instructor.User.LastName.Contains(search)||
                               (c.Instructor.User.FirstName+" "+ c.Instructor.User.LastName).Contains(search)));
            }

            // cources that in a specific category
            if (!string.IsNullOrEmpty(category) && category != "All Categories")
            {
                var category_id = await _context.Categories.Where(c => c.Name == category)
                                                .Select(c => c.CategoryId).FirstOrDefaultAsync();
                
                query = query.Where(c => c.Categories.Any(cat => cat.CategoryId == category_id));
            }

            var count = await query.CountAsync();

            var data = await query.Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .Select(c => new 
                                           {
                                               Id = c.Id,
                                               Title = c.Title,
                                               InstructorFirstName = c.Instructor.User.FirstName,
                                               InstructorLastName = c.Instructor.User.LastName,
                                               Categories = c.Categories.Select(c=>c.Name).ToList(),
                                               CreatedDate = c.CreatedDate,
                                               StudentsCount = c.Enrollments.Count()
                                           }).ToListAsync();
            var coursesDtos = data.Select(c => new AdminCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                InstructorName = $"{c.InstructorFirstName} {c.InstructorLastName}",
                Categories = string.Join(", ", c.Categories),
                CreatedDate = c.CreatedDate,
                StudentsCount = c.StudentsCount
            }).ToList();

            return new PagedResult<AdminCourseDto>
            {
                Items = coursesDtos,
                TotalCount = count,
                PageNumber = page,
                PageSize = pageSize,
            };
        }



        public async Task<PagedResult<AdminUserDto>> GetUsersAsync(string search, string role, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsNoTracking().AsQueryable();

            // search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Email.Contains(search) ||
                                         u.FirstName.Contains(search) ||
                                         u.LastName.Contains(search));
            }

            // filter by role
            if (!string.IsNullOrEmpty(role) && role != "all")
            {
                var roleId = await _context.Roles
                    .Where(r => r.Name == role)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (roleId != 0)
                {
                    query = query.Where(u => _context.Set<IdentityUserRole<int>>()
                        .Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
                }
            }

            // Number of total items after filtering
            var totalCount = await query.CountAsync();

            // Pagination & Projection
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email
                }).ToListAsync();

            // Get the user's role and the defualt role is student
            foreach (var userDto in users)
            {
                var userEntity = await _userManager.FindByEmailAsync(userDto.Email);
                var roles = await _userManager.GetRolesAsync(userEntity);
                userDto.Roles = roles.Any() ? roles.ToList() : new List<string> { "Student" };
            }

            return new PagedResult<AdminUserDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
