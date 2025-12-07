using BLL.DTOs.Admin;
using BLL.Interfaces.Admin;
using Core.Entities;
using Core.Entities.Enums;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly INotifier _notifier;
        private readonly static int PageSize = 10;

        public AdminService(UserManager<User> userManager, AppDbContext context, INotifier notifier)
        {
            _userManager = userManager;
            _context = context;
            _notifier = notifier;
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

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return false; 
            }
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


        
        // Pending Courses Methodss
        public async Task<List<AdminPendingCourseDto>> GetPendingCoursesAsync()
        {
            var defaultImg = "https://images.unsplash.com/photo-1587620962725-abab7fe55159?q=80&w=800&auto=format&fit=crop";
            var query = _context.Courses.AsNoTracking()
                                        .Where(c => c.Status == LearningEntityStatus.Pending)
                                        .Include(c=>c.Instructor).ThenInclude(i=>i.User)
                                        .Select(c => new AdminPendingCourseDto
                                        {
                                            Id = c.Id,
                                            Title = c.Title,
                                            Level = c.Level.ToString(),
                                            ThumbnailImageUrl = c.ThumbnailImageUrl ?? defaultImg,
                                            InstructorName = c.Instructor.User.FirstName + " " + c.Instructor.User.LastName,
                                            ModulesCount = c.Modules.Count(),
                                            LessonsCount = c.Modules.SelectMany(m=>m.Lessons).Count(),
                                            InstructorCoursesCount = c.Instructor.OwnedCourses.Count(),
                                            CreatedDate = c.CreatedDate
                                        });
            return await query.ToListAsync();
        }

        public async Task<int> GetPendingCoursesCountAsync()
        {
            return await _context.Courses
                .CountAsync(c => c.Status == LearningEntityStatus.Pending);
        }

        public async Task ApproveCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            var userId = await _context.InstructorProfiles
                            .Where(i => i.InstructorId == course.InstructorId)
                            .Select(i => i.UserId)
                            .FirstOrDefaultAsync();
            if (course == null) return;

            course.Status = LearningEntityStatus.Published;

            
            var notification = new Notification
            {
                UserId = userId,
                Title = "Course Approved! 🎉",
                Message = $"Your course '{course.Title}' is now live.",
                Url = $"/Course/Details/{courseId}",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            
            await _notifier.SendToUserAsync(userId, notification.Title, notification.Message, notification.Url);
        }

        
        public async Task RejectCourseAsync(int courseId, string reason)
        {
            var course = await _context.Courses.FindAsync(courseId);
            var userId = await _context.InstructorProfiles
                            .Where(i => i.InstructorId == course.InstructorId)
                            .Select(i => i.UserId)
                            .FirstOrDefaultAsync();
            if (course == null) return;

            course.Status = LearningEntityStatus.Rejected;

            var notification = new Notification
            {
                UserId = userId,
                Title = "Course Rejected ⚠️",
                Message = $"Your course '{course.Title}' was rejected. Reason: {reason}",
                Url = "/Instructor/Dashboard",
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();


            await _notifier.SendToUserAsync(userId, notification.Title, notification.Message, notification.Url);
        }


        // Tracks Methods 
        public async Task<PagedResult<AdminTrackDto>> GetTracksAsync(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.Tracks
                        .AsNoTracking()
                        .Include(t => t.TrackCourses)
                        .Include(t => t.Enrollments)
                        .AsQueryable();

            // Search by title or description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));
            }

            var count = await query.CountAsync();

            var data = await query.OrderByDescending(t => t.CreatedDate)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .Select(t => new AdminTrackDto
                          {
                              Id = t.Id,
                              Title = t.Title,
                              Description = t.Description,
                              CoursesCount = t.TrackCourses != null ? t.TrackCourses.Count : 0,
                              StudentsCount = t.Enrollments != null ? t.Enrollments.Count : 0,
                              CreatedDate = t.CreatedDate
                          }).ToListAsync();

            return new PagedResult<AdminTrackDto>
            {
                Items = data,
                TotalCount = count,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteTrackAsync(int trackId)
        {
            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null) return false;

            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CourseSimpleDto>> GetAllCoursesSimpleAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                // Remove the Published filter to show all courses
                .OrderBy(c => c.Title)
                .Select(c => new CourseSimpleDto
                {
                    Id = c.Id,
                    Title = c.Title
                })
                .ToListAsync();
        }

        public async Task<int> CreateTrackAsync(CreateTrackDto dto)
        {
            var track = new Core.Entities.Track
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = (LearningEntityStatus)dto.Status,
                CreatedDate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();

            // Add Courses to Track
            if (dto.CourseIds.Count > 0)
            {
                foreach (var courseId in dto.CourseIds)
                {
                    _context.TrackCourses.Add(new Track_Course
                    {
                        TrackId = track.Id,
                        CourseId = courseId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return track.Id;
        }
    }
}
