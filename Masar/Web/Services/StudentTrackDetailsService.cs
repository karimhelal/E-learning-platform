using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Web.ViewModels.Student;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Services
{
    public class StudentTrackDetailsService : IStudentTrackDetailsService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<StudentTrackDetailsService> _logger;

        public StudentTrackDetailsService(IUserRepository userRepo, ILogger<StudentTrackDetailsService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<StudentTrackDetailsData?> GetTrackDetailsAsync(int studentId, int trackId)
        {
            try
            {
                _logger.LogInformation("Fetching track details for studentId: {StudentId}, trackId: {TrackId}", studentId, trackId);

                // FIXED: Use GetStudentProfileAsync instead of GetStudentProfileForUserAsync
                // The controller passes StudentId, not UserId
                var student = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

                if (student == null)
                {
                    _logger.LogWarning("Student profile not found for studentId: {StudentId}", studentId);
                    return null;
                }

                _logger.LogInformation("Student found: {Name}, Enrollments count: {Count}", 
                    student.User?.FirstName, student.Enrollments?.Count ?? 0);

                // Find the track enrollment from the generic Enrollments collection
                var te = student.Enrollments?
                    .OfType<TrackEnrollment>()
                    .FirstOrDefault(e => e.TrackId == trackId);
                
                if (te == null || te.Track == null)
                {
                    _logger.LogWarning("Track enrollment not found for trackId: {TrackId}", trackId);
                    return null;
                }

                var track = te.Track;

                var dto = new StudentTrackDetailsData
                {
                    StudentId = student.StudentId,
                    StudentName = student.User?.FirstName ?? "Student",
                    UserInitials = GetInitials($"{student.User?.FirstName ?? ""} {student.User?.LastName ?? ""}"),
                    TrackId = track.Id,
                    Title = track.Title,
                    Description = track.Description ?? "",
                    Tags = track.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
                    TotalProgress = te.ProgressPercentage
                };

                // Build course items
                dto.Courses = (track.TrackCourses ?? new List<Track_Course>())
                    .Select(tc =>
                    {
                        var course = tc.Course!;
                        var courseEnrollment = student.Enrollments?
                            .OfType<CourseEnrollment>()
                            .FirstOrDefault(c => c.CourseId == course.Id);

                        decimal prog = courseEnrollment?.ProgressPercentage ?? 0m;
                        int lessons = course.Modules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0;
                        int duration = CalculateCourseDurationHours(course);

                        return new TrackCourseItem
                        {
                            CourseId = course.Id,
                            Title = course.Title,
                            Description = course.Description ?? "",
                            LessonsCount = lessons,
                            DurationHours = duration,
                            ProgressPercentage = prog
                        };
                    })
                    .ToList();

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

            var totalSeconds = course.Modules
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Select(l => l.LessonContent)
                .OfType<VideoContent>()
                .Sum(v => v.DurationInSeconds);

            return (int)System.Math.Ceiling(totalSeconds / 3600.0);
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "JD";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();

            return parts.Length > 0 ? parts[0][0].ToString().ToUpper() : "JD";
        }
    }
}
