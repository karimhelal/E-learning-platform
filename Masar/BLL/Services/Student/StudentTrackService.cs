namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

public class StudentTracksService : IStudentTrackService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentTracksService> _logger;

    public StudentTracksService(IUserRepository userRepo, ILogger<StudentTracksService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentTracksDto?> GetStudentTracksAsync(int studentId)
    {
        try
        {
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile?.User == null) return null;

            var trackEnrollments = studentProfile.Enrollments
                .OfType<TrackEnrollment>()
                .Where(e => e.Track != null)
                .ToList();

            var mappedTracks = MapTracks(trackEnrollments, studentId);

            return new StudentTracksDto
            {
                StudentId = studentProfile.StudentId,
                StudentName = studentProfile.User.FirstName,
                UserInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}"),
                Tracks = mappedTracks,
                Stats = new TrackPageStatsDto
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

    private List<StudentTrackItemDto> MapTracks(List<TrackEnrollment> enrollments, int studentId)
    {
        return enrollments
            .OrderByDescending(e => e.EnrollmentDate)
            .Select(e =>
            {
                var track = e.Track!;
                var courses = track.TrackCourses?.Where(tc => tc.Course != null).Select(tc => tc.Course!).ToList() ?? new List<Course>();

                var totalCourses = courses.Count;
                var completedCourses = courses.Count(c => c.Enrollments?.Any(ce => ce.StudentId == studentId && ce.ProgressPercentage >= 100) ?? false);

                decimal progress = totalCourses == 0 ? 0 : Math.Round((decimal)completedCourses / totalCourses * 100, 1);

                return new StudentTrackItemDto
                {
                    TrackId = track.Id,
                    Title = track.Title,
                    Description = track.Description ?? "",
                    CoursesCount = totalCourses,
                    DurationHours = CalculateTrackDuration(courses),
                    ProgressPercentage = progress,
                    Status = progress >= 100 ? "completed" : "in-progress",
                    IconClass = "fa-laptop-code",
                    ActionText = progress >= 100 ? "View Certificate" : "Continue Learning",
                    ActionUrl = $"/Student/Track/{track.Id}",
                    Courses = MapTrackCourses(courses, studentId)
                };
            })
            .ToList();
    }

    private List<TrackCoursePreviewDto> MapTrackCourses(List<Course> courses, int studentId)
    {
        return courses.Select(c => new TrackCoursePreviewDto
        {
            CourseId = c.Id,
            Title = c.Title,
            IconClass = "fa-book",
            Status = (c.Enrollments?.Any(e => e.StudentId == studentId && e.ProgressPercentage >= 100) ?? false) ? "completed" : "in-progress"
        }).ToList();
    }

    private int CalculateTrackDuration(List<Course> courses)
    {
        if (courses == null || !courses.Any()) return 0;
        var totalSeconds = courses.Where(c => c.Modules != null).SelectMany(c => c.Modules!).SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}