using Core.Entities;
using Core.RepositoryInterfaces;
using Web.Interfaces;

namespace Web.Services;

/// <summary>
/// Your simplified Student Dashboard service
/// Wraps complex BLL logic for easy controller usage
/// </summary>
public class StudentDashboardService : IStudentDashboardService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentDashboardService> _logger;

    public StudentDashboardService(
        IUserRepository userRepo,
        ILogger<StudentDashboardService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentDashboardData?> GetDashboardDataAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard data for student ID: {StudentId}", studentId);
            
            // CHANGED: Use GetStudentProfileAsync instead of GetStudentProfileForUserAsync
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile == null)
            {
                _logger.LogWarning("Student profile is NULL for ID: {StudentId}", studentId);
                return null;
            }

            if (studentProfile.User == null)
            {
                _logger.LogWarning("Student profile found but User is NULL for ID: {StudentId}", studentId);
                return null;
            }

            _logger.LogInformation("Student profile loaded successfully. User: {FirstName} {LastName}", 
                studentProfile.User.FirstName, studentProfile.User.LastName);

            var user = studentProfile.User;

            // Get enrollments - handle null case
            var enrollments = studentProfile.Enrollments ?? new HashSet<EnrollmentBase>();
            _logger.LogInformation("Total enrollments loaded: {Count}", enrollments.Count);
            
            var courseEnrollments = enrollments
                .OfType<CourseEnrollment>()
                .Where(e => e.Course != null)
                .ToList();

            _logger.LogInformation("Course enrollments: {Count}", courseEnrollments.Count);

            var trackEnrollments = enrollments
                .OfType<TrackEnrollment>()
                .Where(e => e.Track != null)
                .ToList();

            _logger.LogInformation("Track enrollments: {Count}", trackEnrollments.Count);

            // Calculate stats
            var stats = CalculateStats(studentProfile, courseEnrollments, trackEnrollments);
            _logger.LogInformation("Stats calculated - Active Tracks: {ActiveTracks}, Enrolled Courses: {EnrolledCourses}", 
                stats.ActiveTracks, stats.EnrolledCourses);

            // Get in-progress courses
            var continueLearningCourses = GetContinueLearningCourses(courseEnrollments);
            _logger.LogInformation("Continue learning courses: {Count}", continueLearningCourses.Count);

            // Get enrolled tracks
            var enrolledTracks = GetEnrolledTracks(trackEnrollments);
            _logger.LogInformation("Enrolled tracks: {Count}", enrolledTracks.Count);

            return new StudentDashboardData
            {
                StudentId = studentProfile.StudentId,
                StudentName = user.FirstName,
                UserInitials = GetInitials($"{user.FirstName} {user.LastName}"),
                Stats = stats,
                ContinueLearningCourses = continueLearningCourses,
                EnrolledTracks = enrolledTracks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for student {StudentId}. Exception: {Message}", studentId, ex.Message);
            throw; // Re-throw to see the actual error
        }
    }

    private DashboardStats CalculateStats(
        StudentProfile studentProfile,
        List<CourseEnrollment> courseEnrollments,
        List<TrackEnrollment> trackEnrollments)
    {
        var completedCourses = courseEnrollments.Count(e => e.ProgressPercentage >= 100);
        var certificatesCount = studentProfile.Certificates?.Count() ?? 0;

        return new DashboardStats
        {
            ActiveTracks = trackEnrollments.Count,
            EnrolledCourses = courseEnrollments.Count,
            CompletedCourses = completedCourses,
            CertificatesEarned = certificatesCount
        };
    }

    private List<ContinueLearningCourse> GetContinueLearningCourses(List<CourseEnrollment> courseEnrollments)
    {
        // Map category names to icons and badge classes
        var categoryMap = new Dictionary<string, (string icon, string badge)>
        {
            { "Web Development", ("fa-laptop-code", "badge-purple") },
            { "Data Science", ("fa-brain", "badge-cyan") },
            { "Mobile Development", ("fa-mobile-alt", "badge-green") },
            { "DevOps", ("fa-server", "badge-orange") },
            { "Design", ("fa-palette", "badge-pink") }
        };

        return courseEnrollments
            .Where(e => e.ProgressPercentage < 100 && e.ProgressPercentage > 0)
            .OrderByDescending(e => e.ProgressPercentage)
            .Take(2)
            .Select(e =>
            {
                var categoryName = e.Course?.Categories?.FirstOrDefault()?.Name ?? "General";
                var (icon, badge) = categoryMap.ContainsKey(categoryName)
                    ? categoryMap[categoryName]
                    : ("fa-book", "badge-purple");

                return new ContinueLearningCourse
                {
                    CourseId = e.Course!.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description ?? string.Empty,
                    ThumbnailImageUrl = e.Course.ThumbnailImageUrl,
                    CategoryName = categoryName,
                    CategoryIcon = icon,
                    CategoryBadgeClass = badge,
                    InstructorName = e.Course.Instructor?.User?.FirstName ?? "Instructor",
                    ProgressPercentage = e.ProgressPercentage,
                    DurationHours = CalculateCourseDuration(e.Course)
                };
            })
            .ToList();
    }

    private List<EnrolledTrack> GetEnrolledTracks(List<TrackEnrollment> trackEnrollments)
    {
        var icons = new[] { "fa-laptop-code", "fa-brain", "fa-mobile-alt", "fa-server" };
        var styles = new[]
        {
            "",
            "background: rgba(6, 182, 212, 0.15); color: var(--accent-cyan);",
            "background: rgba(16, 185, 129, 0.15); color: var(--accent-green);",
            "background: rgba(245, 158, 11, 0.15); color: var(--accent-orange);"
        };

        return trackEnrollments
            .OrderByDescending(e => e.ProgressPercentage)
            .Take(2)
            .Select((e, index) => new EnrolledTrack
            {
                TrackId = e.Track!.Id,
                Title = e.Track.Title,
                Description = e.Track.Description ?? string.Empty,
                CoursesCount = e.Track.TrackCourses?.Count ?? 0,
                TotalHours = CalculateTrackDuration(e.Track),
                ProgressPercentage = e.ProgressPercentage,
                IconClass = icons[index % icons.Length],
                IconStyle = styles[index % styles.Length]
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

    private int CalculateTrackDuration(Track track)
    {
        if (track.TrackCourses == null) return 0;

        var totalSeconds = track.TrackCourses
            .Select(tc => tc.Course)
            .Where(c => c != null)
            .SelectMany(c => c!.Modules ?? new List<Module>())
            .SelectMany(m => m.Lessons ?? new List<Lesson>())
            .Select(l => l.LessonContent)
            .OfType<VideoContent>()
            .Sum(v => v.DurationInSeconds);

        return (int)Math.Ceiling(totalSeconds / 3600.0);
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