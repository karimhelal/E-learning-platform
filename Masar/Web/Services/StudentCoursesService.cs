using Core.Entities;
using Core.RepositoryInterfaces;
using Web.Interfaces;

namespace Web.Services;

/// <summary>
/// Service for handling Student My Courses functionality
/// </summary>
public class StudentCoursesService : IStudentCoursesService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentCoursesService> _logger;

    public StudentCoursesService(
        IUserRepository userRepo,
        ILogger<StudentCoursesService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentCoursesData?> GetMyCoursesAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Fetching courses for student ID: {StudentId}", userId);
            
            var studentProfile = await _userRepo.GetStudentProfileAsync(userId, includeUserBase: true);

            if (studentProfile == null || studentProfile.User == null)
            {
                _logger.LogWarning("Student profile not found for ID: {StudentId}", userId);
                return null;
            }

            var courseEnrollments = studentProfile.Enrollments
                .OfType<CourseEnrollment>()
                .Where(e => e.Course != null)
                .ToList();

            _logger.LogInformation("Found {Count} course enrollments", courseEnrollments.Count);

            var allCourses = MapCourseEnrollments(courseEnrollments);
            var inProgressCourses = allCourses.Where(c => c.Status == "InProgress").ToList();
            var completedCourses = allCourses.Where(c => c.Status == "Completed").ToList();

            return new StudentCoursesData
            {
                StudentId = studentProfile.StudentId,
                StudentName = studentProfile.User.FirstName,
                UserInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}"), // ADD THIS LINE
                AllCourses = allCourses,
                InProgressCourses = inProgressCourses,
                CompletedCourses = completedCourses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for student {StudentId}", userId);
            return null;
        }
    }

    private List<MyCourseItem> MapCourseEnrollments(List<CourseEnrollment> enrollments)
    {
        var categoryMap = new Dictionary<string, (string icon, string badge)>
        {
            { "Web Development", ("fa-laptop-code", "badge-purple") },
            { "Data Science", ("fa-brain", "badge-cyan") },
            { "Mobile Development", ("fa-mobile-alt", "badge-green") },
            { "Programming", ("fa-code", "badge-green") },
            { "Design", ("fa-paint-brush", "badge-purple") },
            { "DevOps", ("fa-server", "badge-orange") }
        };

        return enrollments
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e =>
            {
                var course = e.Course!;
                var categoryName = course.Categories?.FirstOrDefault()?.Name ?? "General";
                var (icon, badge) = categoryMap.ContainsKey(categoryName)
                    ? categoryMap[categoryName]
                    : ("fa-book", "badge-purple");

                var totalLessons = course.Modules?
                    .SelectMany(m => m.Lessons ?? new List<Lesson>())
                    .Count() ?? 0;

                var completedLessons = (int)(totalLessons * (e.ProgressPercentage / 100m));

                return new MyCourseItem
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description ?? string.Empty,
                    CategoryName = categoryName,
                    CategoryIcon = icon,
                    CategoryBadgeClass = badge,
                    InstructorName = course.Instructor?.User?.FirstName ?? "Instructor",
                    ModulesCount = course.Modules?.Count ?? 0,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    DurationHours = CalculateCourseDuration(course),
                    ProgressPercentage = e.ProgressPercentage,
                    Status = e.ProgressPercentage >= 100 ? "Completed" : "InProgress",
                    EnrollmentDate = e.EnrollmentDate ?? DateTime.Now,
                    CompletionDate = e.ProgressPercentage >= 100 ? DateTime.Now : null
                };
            })
            .ToList();
    }

    private int CalculateCourseDuration(Course course)
    {
        if (course.Modules == null) return 0;

        var totalSeconds = course.Modules
            .SelectMany(m => m.Lessons ?? new List<Lesson>())
            .Select(l => l.LessonContent)
            .OfType<VideoContent>()
            .Sum(v => v.DurationInSeconds);

        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    // ADD THIS HELPER METHOD at the end of the class
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