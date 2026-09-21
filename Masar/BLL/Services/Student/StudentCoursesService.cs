namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

public class StudentCoursesService : IStudentCoursesService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentCoursesService> _logger;

    public StudentCoursesService(IUserRepository userRepo, ILogger<StudentCoursesService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentCoursesDto?> GetMyCoursesAsync(int studentId)
    {
        try
        {
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile?.User == null) return null;

            var courseEnrollments = studentProfile.Enrollments
                .OfType<CourseEnrollment>()
                .Where(e => e.Course != null)
                .ToList();

            var allCourses = MapCourseEnrollments(courseEnrollments);
            var inProgressCourses = allCourses.Where(c => c.Status == "InProgress").ToList();
            var completedCourses = allCourses.Where(c => c.Status == "Completed").ToList();

            return new StudentCoursesDto
            {
                StudentId = studentProfile.StudentId,
                StudentName = studentProfile.User.FirstName,
                UserInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}"),
                AllCourses = allCourses,
                InProgressCourses = inProgressCourses,
                CompletedCourses = completedCourses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for student {StudentId}", studentId);
            return null;
        }
    }

    private List<MyCourseItemDto> MapCourseEnrollments(List<CourseEnrollment> enrollments)
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
                var (icon, badge) = categoryMap.ContainsKey(categoryName) ? categoryMap[categoryName] : ("fa-book", "badge-purple");

                var totalLessons = course.Modules?.SelectMany(m => m.Lessons ?? new List<Lesson>()).Count() ?? 0;
                var completedLessons = (int)(totalLessons * (e.ProgressPercentage / 100m));

                return new MyCourseItemDto
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description ?? string.Empty,
                    ThumbnailImageUrl = course.ThumbnailImageUrl,
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

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}