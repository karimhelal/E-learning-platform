using Core.Entities;
using Core.RepositoryInterfaces;
using Web.Interfaces;

namespace Web.Services;

/// <summary>
/// Service for handling Student "My Tracks" functionality
/// </summary>
public class StudentTracksService : IStudentTrackService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentTracksService> _logger;

    public StudentTracksService(
        IUserRepository userRepo,
        ILogger<StudentTracksService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentTracksData?> GetStudentTracksAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Fetching Tracks for student ID: {StudentId}", studentId);

            // CHANGED: Use GetStudentProfileAsync instead of GetStudentProfileForUserAsync
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile == null || studentProfile.User == null)
            {
                _logger.LogWarning("Student profile not found for ID: {StudentId}", studentId);
                return null;
            }

            _logger.LogInformation("Student profile loaded. User: {FirstName} {LastName}", 
                studentProfile.User.FirstName, studentProfile.User.LastName);
            _logger.LogInformation("Total enrollments: {Count}", studentProfile.Enrollments?.Count ?? 0);

            var trackEnrollments = studentProfile.Enrollments
                .OfType<TrackEnrollment>()
                .Where(e => e.Track != null)
                .ToList();

            _logger.LogInformation("Found {Count} track enrollments", trackEnrollments.Count);

            var mappedTracks = MapTracks(trackEnrollments, studentId);

            return new StudentTracksData
            {
                StudentId = studentProfile.StudentId,
                StudentName = studentProfile.User.FirstName,
                UserInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}"),
                Tracks = mappedTracks,
                Stats = new TrackPageStats
                {
                    TotalTracks = mappedTracks.Count,
                    InProgressTracks = mappedTracks.Count(t => t.Status == "in-progress"),
                    CompletedTracks = mappedTracks.Count(t => t.Status == "completed")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracks for student {StudentId}", studentId);
            return null;
        }
    }

    // ---------------------------------------------------------------
    // MAPPING SECTION
    // ---------------------------------------------------------------

    private List<StudentTrackItem> MapTracks(List<TrackEnrollment> enrollments, int studentId)
    {
        return enrollments
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e =>
            {
                var track = e.Track!;
                var courses = track.TrackCourses?
                    .Where(tc => tc.Course != null)
                    .Select(tc => tc.Course!)
                    .ToList() ?? new List<Course>();

                // Calculate progress
                var totalCourses = courses.Count;
                var completedCourses = courses
                    .Count(c => c.Enrollments?.Any(ce => ce.StudentId == studentId && ce.ProgressPercentage >= 100) ?? false);

                decimal progress = totalCourses == 0
                    ? 0
                    : Math.Round((decimal)completedCourses / totalCourses * 100, 1);

                // Calculate total duration
                int durationHours = CalculateTrackDuration(courses);

                return new StudentTrackItem
                {
                    TrackId = track.Id,
                    Title = track.Title,
                    Description = track.Description ?? "",
                    CoursesCount = totalCourses,
                    DurationHours = durationHours,
                    ProgressPercentage = progress,
                    Status = progress >= 100 ? "completed" : "in-progress",
                    IconClass = "fa-laptop-code", // you can change based on track
                    ActionText = progress >= 100 ? "View Certificate" : "Continue Learning",
                    ActionUrl = $"/Student/Track/{track.Id}",
                    Courses = MapTrackCourses(courses, studentId)
                };
            })
            .ToList();
    }

    private List<TrackCoursePreview> MapTrackCourses(List<Course> courses, int studentId)
    {
        return courses.Select(c =>
        {
            var isCompleted = c.Enrollments?
                .Any(e => e.StudentId == studentId && e.ProgressPercentage >= 100) ?? false;

            return new TrackCoursePreview
            {
                CourseId = c.Id,
                Title = c.Title,
                IconClass = "fa-book",
                Status = isCompleted ? "completed" : "in-progress"
            };
        })
        .ToList();
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------

    private int CalculateTrackDuration(List<Course> courses)
    {
        if (courses == null || !courses.Any()) return 0;

        var totalSeconds = courses
            .Where(c => c.Modules != null)
            .SelectMany(c => c.Modules!)
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
