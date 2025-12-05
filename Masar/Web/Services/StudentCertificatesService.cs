using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;

namespace Web.Services;

public class StudentCertificatesService : IStudentCertificatesService
{
    private readonly IUserRepository _userRepo;
    private readonly ICertificateGenerationService _certificateGenerator;
    private readonly ILogger<StudentCertificatesService> _logger;

    public StudentCertificatesService(
        IUserRepository userRepo,
        ICertificateGenerationService certificateGenerator, // Add this
        ILogger<StudentCertificatesService> logger)
    {
        _userRepo = userRepo;
        _certificateGenerator = certificateGenerator; // Add this
        _logger = logger;
    }

    public async Task<StudentCertificatesData?> GetStudentCertificatesAsync(int studentId)
    {
        try
        {
            // FIRST: Generate any missing certificates for completed courses/tracks
            var generatedCount = await _certificateGenerator.GenerateMissingCertificatesAsync(studentId);
            if (generatedCount > 0)
            {
                _logger.LogInformation(
                    "Generated {Count} missing certificates for student {StudentId}", 
                    generatedCount, studentId);
            }

            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            if (studentProfile?.User == null)
            {
                _logger.LogWarning("Student profile not found for ID: {StudentId}", studentId);
                return null;
            }

            var user = studentProfile.User;

            // Get all certificates (now including any newly generated ones)
            var courseCertificates = studentProfile.Certificates
                .OfType<Core.Entities.CourseCertificate>()
                .Where(c => c.Course != null)
                .Select(c => new CertificateItem
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
                .Select(c => new CertificateItem
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

            return new StudentCertificatesData
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
        if (string.IsNullOrWhiteSpace(name))
            return "JD";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return parts[0][0].ToString().ToUpper();
    }
}