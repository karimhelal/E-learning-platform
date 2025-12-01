using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Student;

public class StudentProfileService : IStudentProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StudentProfileService> _logger;

    public StudentProfileService(IUserRepository userRepository, ILogger<StudentProfileService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<StudentProfileDto?> GetStudentProfileAsync(int studentId)
    {
        try
        {
            var studentProfile = await _userRepository.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile == null || studentProfile.User == null)
            {
                _logger.LogWarning("Student profile not found for StudentId: {StudentId}", studentId);
                return null;
            }

            var user = studentProfile.User;

            // Calculate statistics
            var courseEnrollments = studentProfile.Enrollments
                .OfType<Core.Entities.CourseEnrollment>()
                .Where(e => e.Course != null)
                .ToList();

            var completedCourses = courseEnrollments.Count(e => e.ProgressPercentage >= 100);
            var activeCourses = courseEnrollments.Count(e => e.ProgressPercentage > 0 && e.ProgressPercentage < 100);
            var certificatesCount = studentProfile.Certificates?.Count() ?? 0;

            // Calculate total learning hours from course durations
            var totalLearningHours = courseEnrollments
                .Where(e => e.Course?.Modules != null)
                .Sum(e => e.Course.Modules
                    .SelectMany(m => m.Lessons ?? new List<Core.Entities.Lesson>())
                    .Select(l => l.LessonContent)
                    .OfType<Core.Entities.VideoContent>()
                    .Sum(v => v.DurationInSeconds)) / 3600;

            return new StudentProfileDto
            {
                StudentId = studentProfile.StudentId,
                UserId = studentProfile.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Phone = null, // Add if you have phone field
                ProfilePicture = user.Picture,
                Bio = studentProfile.Bio,
                JoinedDate = DateTime.Now.AddMonths(-6), // TODO: Get actual joined date from User entity
                Location = null, // Add if you have location field
                Languages = null, // Add if you have languages field
                GithubUrl = null, // Add if you have social links
                LinkedInUrl = null,
                TwitterUrl = null,
                WebsiteUrl = null,
                TotalCoursesEnrolled = courseEnrollments.Count,
                CompletedCourses = completedCourses,
                ActiveCourses = activeCourses,
                CertificatesEarned = certificatesCount,
                TotalLearningHours = totalLearningHours,
                CurrentStreak = 0, // TODO: Calculate actual streak
                LearningInterests = GetLearningInterests(courseEnrollments),
                EnrolledCourses = courseEnrollments
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(6)
                    .Select(e => new StudentProfileCourseDto
                    {
                        CourseId = e.Course!.Id,
                        Title = e.Course.Title,
                        Description = e.Course.Description ?? string.Empty,
                        ThumbnailImageUrl = e.Course.ThumbnailImageUrl,
                        ProgressPercentage = e.ProgressPercentage,
                        Status = e.Status.ToString(),
                        InstructorName = e.Course.Instructor?.User?.FirstName ?? "Instructor"
                    })
                    .ToList(),
                Certificates = studentProfile.Certificates?
                    .OrderByDescending(c => c.IssuedDate)
                    .Select(c => new StudentCertificateDto
                    {
                        // FIXED: Use CertificateId instead of CourseCertificateId/TrackCertificateId
                        CertificateId = c.CertificateId,
                        Title = c is Core.Entities.CourseCertificate courseCert ? courseCert.Course?.Title ?? "Certificate" :
                               (c as Core.Entities.TrackCertificate)?.Track?.Title ?? "Certificate",
                        // FIXED: Convert DateOnly to DateTime
                        IssuedDate = c.IssuedDate.ToDateTime(TimeOnly.MinValue),
                        Type = c is Core.Entities.CourseCertificate ? "Course" : "Track"
                    })
                    .ToList() ?? new List<StudentCertificateDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student profile for StudentId: {StudentId}", studentId);
            return null;
        }
    }

    private List<string> GetLearningInterests(List<Core.Entities.CourseEnrollment> enrollments)
    {
        return enrollments
            .SelectMany(e => e.Course?.Categories ?? new List<Core.Entities.Category>())
            .Select(c => c.Name)
            .Distinct()
            .Take(5)
            .ToList();
    }
}
