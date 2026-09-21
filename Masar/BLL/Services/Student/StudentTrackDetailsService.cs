namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

public class StudentTrackDetailsService : IStudentTrackDetailsService
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<StudentTrackDetailsService> _logger;

    public StudentTrackDetailsService(IUserRepository userRepo, ILogger<StudentTrackDetailsService> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<StudentTrackDetailsDto?> GetTrackDetailsAsync(int studentId, int trackId)
    {
        try
        {
            var student = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (student == null) return null;

            var te = student.Enrollments?.OfType<TrackEnrollment>().FirstOrDefault(e => e.TrackId == trackId);

            if (te == null || te.Track == null) return null;

            var track = te.Track;

            var dto = new StudentTrackDetailsDto
            {
                StudentId = student.StudentId,
                StudentName = student.User?.FirstName ?? "Student",
                UserInitials = GetInitials($"{student.User?.FirstName ?? ""} {student.User?.LastName ?? ""}"),
                TrackId = track.Id,
                Title = track.Title,
                Description = track.Description ?? "",
                Tags = track.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
                TotalProgress = te.ProgressPercentage,
                Courses = (track.TrackCourses ?? new List<Track_Course>()).Select(tc =>
                {
                    var course = tc.Course!;
                    var courseEnrollment = student.Enrollments?.OfType<CourseEnrollment>().FirstOrDefault(c => c.CourseId == course.Id);

                    return new TrackCourseItemDto
                    {
                        CourseId = course.Id,
                        Title = course.Title,
                        Description = course.Description ?? "",
                        LessonsCount = course.Modules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0,
                        DurationHours = CalculateCourseDurationHours(course),
                        ProgressPercentage = courseEnrollment?.ProgressPercentage ?? 0m
                    };
                }).ToList()
            };

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTrackDetailsAsync for studentId: {StudentId}, trackId: {TrackId}", studentId, trackId);
            return null;
        }
    }

    private int CalculateCourseDurationHours(Course course)
    {
        if (course.Modules == null) return 0;
        var totalSeconds = course.Modules.SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}