using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services.Student;

public class StudentProfileService : IStudentProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentProfileService> _logger;

    public StudentProfileService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<StudentProfileService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
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

            // Get skills from Skills table (only Student type)
            var skills = user.Skills?
                .Where(s => s.SkillType == "Student")
                .Select(s => s.SkillName)
                .ToList() ?? new List<string>();

            return new StudentProfileDto
            {
                StudentId = studentProfile.StudentId,
                UserId = studentProfile.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                ProfilePicture = user.Picture,
                Bio = studentProfile.Bio,
                JoinedDate = DateTime.Now.AddMonths(-6),
                Location = studentProfile.Location,
                GithubUrl = studentProfile.GithubUrl,
                LinkedInUrl = studentProfile.LinkedInUrl,
                FacebookUrl = studentProfile.FacebookUrl,
                WebsiteUrl = studentProfile.WebsiteUrl,
                TotalCoursesEnrolled = courseEnrollments.Count,
                CompletedCourses = completedCourses,
                ActiveCourses = activeCourses,
                CertificatesEarned = certificatesCount,
                TotalLearningHours = totalLearningHours,
                CurrentStreak = 0,
                Skills = skills,
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
                        CertificateId = c.CertificateId,
                        Title = c is Core.Entities.CourseCertificate courseCert ? courseCert.Course?.Title ?? "Certificate" :
                               (c as Core.Entities.TrackCertificate)?.Track?.Title ?? "Certificate",
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

    public async Task<bool> UpdateStudentProfileAsync(int studentId, UpdateStudentProfileDto profileDto)
    {
        try
        {
            var studentProfile = await _userRepository.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile == null || studentProfile.User == null)
            {
                _logger.LogWarning("Student profile not found for update. StudentId: {StudentId}", studentId);
                return false;
            }

            // Update student profile fields
            studentProfile.Bio = profileDto.Bio;
            studentProfile.Location = profileDto.Location;
            studentProfile.GithubUrl = profileDto.GithubUrl;
            studentProfile.LinkedInUrl = profileDto.LinkedInUrl;
            studentProfile.FacebookUrl = profileDto.FacebookUrl;
            studentProfile.WebsiteUrl = profileDto.WebsiteUrl;

            // Update user info
            studentProfile.User.FirstName = profileDto.FirstName;
            studentProfile.User.LastName = profileDto.LastName;
            studentProfile.User.PhoneNumber = profileDto.Phone;
            
            // Update profile picture if provided
            if (!string.IsNullOrEmpty(profileDto.ProfilePictureUrl))
            {
                studentProfile.User.Picture = profileDto.ProfilePictureUrl;
            }

            // Update skills (saved as Skills in database with SkillType = Student)
            if (profileDto.Skills != null)
            {
                // Remove existing student skills only
                var existingStudentSkills = studentProfile.User.Skills?
                    .Where(s => s.SkillType == "Student")
                    .ToList() ?? new List<Core.Entities.Skill>();
                
                foreach (var skill in existingStudentSkills)
                {
                    studentProfile.User.Skills?.Remove(skill);
                }
                
                // Add new skills with SkillType = Student
                foreach (var skillName in profileDto.Skills.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    studentProfile.User.Skills?.Add(new Core.Entities.Skill
                    {
                        SkillName = skillName.Trim(),
                        UserId = studentProfile.UserId,
                        SkillType = "Student"
                    });
                }
            }

            // Save changes using Unit of Work
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student profile updated successfully. StudentId: {StudentId}", studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student profile for StudentId: {StudentId}", studentId);
            return false;
        }
    }
}
