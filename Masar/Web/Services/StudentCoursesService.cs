using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;

namespace Web.Services;

/// <summary>
/// Service for handling Student My Courses functionality
/// Completely remade to properly read from database
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
            _logger.LogInformation("?? Fetching courses for user ID: {UserId}", userId);
            
            // Fetch student profile with all related data
            var studentProfile = await _userRepo.GetStudentProfileForUserAsync(userId, includeUserBase: true);

            if (studentProfile == null)
            {
                _logger.LogWarning("?? Student profile not found for user ID: {UserId}", userId);
                return null;
            }

            if (studentProfile.User == null)
            {
                _logger.LogWarning("?? User data not found for student profile ID: {StudentId}", studentProfile.StudentId);
                return null;
            }

            // Filter only course enrollments (not track enrollments)
            var courseEnrollments = studentProfile.Enrollments?
                .OfType<CourseEnrollment>()
                .Where(e => e.Course != null)
                .ToList() ?? new List<CourseEnrollment>();

            _logger.LogInformation("? Found {Count} course enrollments for user {UserId}", 
                courseEnrollments.Count, userId);

            // Map enrollments to view models
            var allCourses = MapCourseEnrollments(courseEnrollments);
            
            // Filter by status
            var notStartedCourses = allCourses
                .Where(c => c.Status == "NotStarted")
                .ToList();
            
            var inProgressCourses = allCourses
                .Where(c => c.Status == "InProgress")
                .ToList();
            
            var completedCourses = allCourses
                .Where(c => c.Status == "Completed")
                .ToList();

            var result = new StudentCoursesData
            {
                StudentId = studentProfile.StudentId,
                StudentName = $"{studentProfile.User.FirstName} {studentProfile.User.LastName}",
                UserInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}"),
                AllCourses = allCourses,
                NotStartedCourses = notStartedCourses,
                InProgressCourses = inProgressCourses,
                CompletedCourses = completedCourses
            };

            _logger.LogInformation(
                "?? Course Stats - Total: {Total}, NotStarted: {NotStarted}, InProgress: {InProgress}, Completed: {Completed}",
                result.AllCoursesCount, result.NotStartedCount, result.InProgressCount, result.CompletedCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error getting courses for user {UserId}", userId);
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
            { "Mobile Apps", ("fa-mobile", "badge-green") },
            { "Programming", ("fa-code", "badge-green") },
            { "Design", ("fa-paint-brush", "badge-purple") },
            { "UI/UX", ("fa-palette", "badge-orange") },
            { "DevOps", ("fa-server", "badge-orange") },
            { "Cloud Computing", ("fa-cloud", "badge-cyan") },
            { "Databases", ("fa-database", "badge-green") },
            { "Machine Learning", ("fa-robot", "badge-purple") },
            { "Cybersecurity", ("fa-shield-alt", "badge-red") }
        };

        return enrollments
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e =>
            {
                var course = e.Course!;
                
                // Get category info
                var mainCategory = course.Categories?.FirstOrDefault();
                var categoryName = mainCategory?.Name ?? "General";
                var (icon, badge) = categoryMap.TryGetValue(categoryName, out var mapping)
                    ? mapping
                    : ("fa-book", "badge-purple");

                // Calculate lesson statistics
                var allLessons = course.Modules?
                    .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
                    .ToList() ?? new List<Lesson>();
                
                var totalLessons = allLessons.Count;
                var completedLessons = CalculateCompletedLessons(e.ProgressPercentage, totalLessons);

                // Calculate duration
                var durationHours = CalculateCourseDuration(course);

                // Determine actual status from database
                var actualStatus = DetermineStatus(e.Status, e.ProgressPercentage);

                // Get instructor name
                var instructorName = course.Instructor?.User != null
                    ? $"{course.Instructor.User.FirstName} {course.Instructor.User.LastName}"
                    : "Unknown Instructor";

                return new MyCourseItem
                {
                    CourseId = course.Id,
                    Title = course.Title ?? "Untitled Course",
                    Description = course.Description ?? "No description available",
                    CategoryName = categoryName,
                    CategoryIcon = icon,
                    CategoryBadgeClass = badge,
                    InstructorName = instructorName,
                    ModulesCount = course.Modules?.Count ?? 0,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    DurationHours = durationHours,
                    ProgressPercentage = e.ProgressPercentage,
                    Status = actualStatus,
                    EnrollmentDate = e.EnrollmentDate ?? DateTime.Now,
                    CompletionDate = actualStatus == "Completed" 
                        ? e.EnrollmentDate?.AddDays(30) // Estimate or store actual completion date
                        : null
                };
            })
            .ToList();
    }

    private string DetermineStatus(EnrollmentStatus dbStatus, decimal progressPercentage)
    {
        // Priority 1: Use database status if it's meaningful
        if (dbStatus == EnrollmentStatus.Completed)
            return "Completed";
        
        if (dbStatus == EnrollmentStatus.Dropped)
            return "Dropped";

        // Priority 2: Use progress percentage
        if (progressPercentage >= 100)
            return "Completed";
        
        if (progressPercentage > 0)
            return "InProgress";

        // Priority 3: Check enrollment status
        if (dbStatus == EnrollmentStatus.NotStarted)
            return "NotStarted";

        // Default for Enrolled status with 0 progress
        return "NotStarted";
    }

    private int CalculateCompletedLessons(decimal progressPercentage, int totalLessons)
    {
        if (totalLessons == 0) return 0;
        
        var completed = (int)Math.Floor(totalLessons * (progressPercentage / 100m));
        return Math.Min(completed, totalLessons); // Ensure not exceeding total
    }

    private int CalculateCourseDuration(Course course)
    {
        if (course.Modules == null || !course.Modules.Any())
            return 0;

        var totalSeconds = course.Modules
            .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
            .Select(l => l.LessonContent)
            .OfType<VideoContent>()
            .Sum(v => v.DurationInSeconds);

        // Convert to hours and round up
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "ST";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        
        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpper();

        return parts[0][0].ToString().ToUpper();
    }
}