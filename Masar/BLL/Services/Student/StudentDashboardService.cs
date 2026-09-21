namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentDashboardService> _logger;

    public StudentDashboardService(IUserRepository userRepo, ILogger<StudentDashboardService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentDashboardDto?> GetDashboardDataAsync(int studentId)
    {
        try
        {
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile?.User == null) return null;

            var user = studentProfile.User;
            var enrollments = studentProfile.Enrollments ?? new HashSet<EnrollmentBase>();

            var courseEnrollments = enrollments.OfType<CourseEnrollment>().Where(e => e.Course != null).ToList();
            var trackEnrollments = enrollments.OfType<TrackEnrollment>().Where(e => e.Track != null).ToList();

            return new StudentDashboardDto
            {
                StudentId = studentProfile.StudentId,
                StudentName = user.FirstName,
                UserInitials = GetInitials($"{user.FirstName} {user.LastName}"),
                Stats = CalculateStats(studentProfile, courseEnrollments, trackEnrollments),
                ContinueLearningCourses = GetContinueLearningCourses(courseEnrollments),
                EnrolledTracks = GetEnrolledTracks(trackEnrollments)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for student {StudentId}", studentId);
            throw;
        }
    }

    private DashboardStatsDto CalculateStats(StudentProfile studentProfile, List<CourseEnrollment> courseEnrollments, List<TrackEnrollment> trackEnrollments)
    {
        return new DashboardStatsDto
        {
            ActiveTracks = trackEnrollments.Count,
            EnrolledCourses = courseEnrollments.Count,
            CompletedCourses = courseEnrollments.Count(e => e.ProgressPercentage >= 100),
            CertificatesEarned = studentProfile.Certificates?.Count() ?? 0
        };
    }

    private List<ContinueLearningCourseDto> GetContinueLearningCourses(List<CourseEnrollment> courseEnrollments)
    {
        var categoryMap = new Dictionary<string, (string icon, string badge)>
        {
            { "Web Development", ("fa-laptop-code", "badge-purple") },
            { "Data Science", ("fa-brain", "badge-cyan") },
            { "Mobile Development", ("fa-mobile-alt", "badge-green") },
            { "DevOps", ("fa-server", "badge-orange") },
            { "Design", ("fa-palette", "badge-pink") }
        };

        return courseEnrollments
            .Where(e => e.ProgressPercentage < 100)
            .OrderByDescending(e => e.ProgressPercentage)
            .ThenByDescending(e => e.EnrollmentDate)
            .Take(2)
            .Select(e =>
            {
                var categoryName = e.Course?.Categories?.FirstOrDefault()?.Name ?? "General";
                var (icon, badge) = categoryMap.ContainsKey(categoryName) ? categoryMap[categoryName] : ("fa-book", "badge-purple");

                return new ContinueLearningCourseDto
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

    private List<EnrolledTrackDto> GetEnrolledTracks(List<TrackEnrollment> trackEnrollments)
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
            .Select((e, index) => new EnrolledTrackDto
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
        var totalSeconds = course.Modules.SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private int CalculateTrackDuration(Track track)
    {
        if (track.TrackCourses == null) return 0;
        var totalSeconds = track.TrackCourses.Select(tc => tc.Course).Where(c => c != null).SelectMany(c => c!.Modules ?? new List<Module>()).SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}