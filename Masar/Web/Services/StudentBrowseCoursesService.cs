using Core.Entities;
using Core.RepositoryInterfaces;
using Core.Entities.Enums;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Web.ViewModels.Student;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
    /// <summary>
    /// Service for retrieving all courses available for students (Browse Courses)
    /// </summary>
    public class StudentBrowseCoursesService : IStudentBrowseCoursesService
    {
        private readonly IUserRepository _userRepo;
        private readonly AppDbContext _context;
        private readonly ILogger<StudentBrowseCoursesService> _logger;

        public StudentBrowseCoursesService(
            IUserRepository userRepo,
            AppDbContext context,
            ILogger<StudentBrowseCoursesService> logger)
        {
            _userRepo = userRepo;
            _context = context;
            _logger = logger;
        }

        public async Task<StudentBrowseCoursesPageData?> GetAllCoursesAsync(int studentId)
        {
            try
            {
                _logger.LogInformation("Fetching browse courses for student: {StudentId}", studentId);

                // Load student profile for name and initials
                var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);
                
                var studentName = "Student";
                var userInitials = "JD";
                
                if (studentProfile?.User != null)
                {
                    studentName = studentProfile.User.FirstName;
                    userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");
                }

                // Fetch actual courses from database
                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                        .ThenInclude(i => i!.User)
                    .Include(c => c.Categories)
                    .Include(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                            .ThenInclude(l => l.LessonContent)
                    .Include(c => c.Enrollments)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} courses in database", courses.Count);

                var mappedCourses = courses.Select(c => MapCourse(c, studentId)).ToList();

                return new StudentBrowseCoursesPageData
                {
                    StudentId = studentId,
                    StudentName = studentName,
                    UserInitials = userInitials,
                    
                    Stats = new BrowseCoursePageStats
                    {
                        TotalCourses = mappedCourses.Count,
                        BeginnerCourses = mappedCourses.Count(c => c.Level == "Beginner"),
                        IntermediateCourses = mappedCourses.Count(c => c.Level == "Intermediate"),
                        AdvancedCourses = mappedCourses.Count(c => c.Level == "Advanced")
                    },

                    Courses = mappedCourses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading browse courses for student {StudentId}", studentId);
                return null;
            }
        }

        private BrowseCourseItem MapCourse(Course course, int studentId)
        {
            var categoryName = course.Categories?.FirstOrDefault()?.Name ?? "General";
            var instructorName = course.Instructor?.User?.FirstName ?? "Instructor";
            
            var totalLessons = course.Modules?
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Count() ?? 0;
            
            var totalSeconds = course.Modules?
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Select(l => l.LessonContent)
                .OfType<VideoContent>()
                .Sum(v => v.DurationInSeconds) ?? 0;
            
            var durationHours = (int)Math.Ceiling(totalSeconds / 3600.0);
            var studentsCount = course.Enrollments?.Count() ?? 0;
            
            // Check if student is enrolled
            var isEnrolled = course.Enrollments?.Any(e => e.StudentId == studentId) ?? false;

            return new BrowseCourseItem
            {
                CourseId = course.Id,
                Title = course.Title,
                Description = course.Description ?? "Learn new skills",
                Level = course.Level.ToString(),
                CategoryName = categoryName,
                CategoryIcon = GetCategoryIcon(categoryName),
                CategoryBadgeClass = GetCategoryBadgeClass(categoryName),
                LevelBadgeClass = course.Level.ToString().ToLower(),
                InstructorName = instructorName,
                ThumbnailImageUrl = course.ThumbnailImageUrl,
                TotalLessons = totalLessons,
                DurationHours = durationHours,
                StudentsCount = studentsCount,
                Rating = 4.8m,
                ActionText = isEnrolled ? "Continue Learning" : "Enroll Now",
                ActionUrl = isEnrolled ? $"/student/course/{course.Id}/details" : $"/student/enroll/course/{course.Id}"
            };
        }

        private string GetCategoryIcon(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "web development" => "fa-laptop-code",
                "data science" => "fa-brain",
                "mobile development" => "fa-mobile-alt",
                "design" => "fa-palette",
                _ => "fa-book"
            };
        }

        private string GetCategoryBadgeClass(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "web development" => "badge-purple",
                "data science" => "badge-cyan",
                "mobile development" => "badge-green",
                "design" => "badge-pink",
                _ => "badge-purple"
            };
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "JD";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();

            return parts[0][0].ToString().ToUpper();
        }
    }
}