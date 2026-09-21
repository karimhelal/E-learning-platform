namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces;
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

public class StudentCertificatesService : IStudentCertificatesService
{
    private readonly IUserRepository _userRepo;
    private readonly ICertificateGenerationService _certificateGenerator;
    private readonly ILogger<StudentCertificatesService> _logger;

    public StudentCertificatesService(
        IUserRepository userRepo,
        ICertificateGenerationService certificateGenerator,
        ILogger<StudentCertificatesService> logger)
    {
        _userRepo = userRepo;
        _certificateGenerator = certificateGenerator;
        _logger = logger;
    }

    public async Task<StudentCertificatesDto?> GetStudentCertificatesAsync(int studentId)
    {
        try
        {
            var generatedCount = await _certificateGenerator.GenerateMissingCertificatesAsync(studentId);
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile?.User == null) return null;

            var user = studentProfile.User;

            var courseCertificates = studentProfile.Certificates
                .OfType<Core.Entities.CourseCertificate>()
                .Where(c => c.Course != null)
                .Select(c => new CertificateItemDto
                {
                    CertificateId = c.CertificateId,
                    Title = c.Title,
                    Type = "Course",
                    IssuedDate = c.IssuedDate.ToString("MMMM dd, yyyy"),
                    CourseName = c.Course!.Title,
                    DownloadLink = c.Link,
                    VerificationId = $"CERT-C-{c.CertificateId:D6}",
                    IsFeatured = false
                })
                .ToList();

            var trackCertificates = studentProfile.Certificates
                .OfType<Core.Entities.TrackCertificate>()
                .Where(c => c.Track != null)
                .Select(c => new CertificateItemDto
                {
                    CertificateId = c.CertificateId,
                    Title = c.Title,
                    Type = "Track",
                    IssuedDate = c.IssuedDate.ToString("MMMM dd, yyyy"),
                    CourseName = c.Track!.Title,
                    DownloadLink = c.Link,
                    VerificationId = $"CERT-T-{c.CertificateId:D6}",
                    IsFeatured = true
                })
                .ToList();

            var allCertificates = trackCertificates
                .Concat(courseCertificates)
                .OrderByDescending(c => c.IsFeatured)
                .ThenByDescending(c => c.IssuedDate)
                .ToList();

            return new StudentCertificatesDto
            {
                StudentId = studentProfile.StudentId,
                StudentName = user.FirstName,
                UserInitials = GetInitials($"{user.FirstName} {user.LastName}"),
                TotalCertificates = allCertificates.Count,
                CourseCertificatesCount = courseCertificates.Count,
                TrackCertificatesCount = trackCertificates.Count,
                Certificates = allCertificates
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificates for student {StudentId}", studentId);
            return null;
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}