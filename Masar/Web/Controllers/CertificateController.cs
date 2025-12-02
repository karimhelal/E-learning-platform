using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using Web.Services;
using Web.ViewModels;

namespace Web.Controllers;

//[AllowAnonymous]
public class CertificateController : Controller
{
    private readonly AppDbContext _context;
    private readonly CertificatePdfGenerator _pdfGenerator;

    public CertificateController(AppDbContext context)
    {
        _context = context;
        _pdfGenerator = new CertificatePdfGenerator();
    }

    [HttpGet("/verify-certificate/{verificationId}")]
    public async Task<IActionResult> VerifyCertificate(string verificationId)
    {
        var viewModel = await GetCertificateViewModel(verificationId);
        
        if (viewModel == null)
        {
            return View("CertificateNotFound");
        }

        return View("VerifyCertificate", viewModel);
    }

    [HttpGet("/download-certificate/{verificationId}")]
    public async Task<IActionResult> DownloadCertificate(string verificationId)
    {
        var viewModel = await GetCertificateViewModel(verificationId);
        
        if (viewModel == null)
        {
            return NotFound("Certificate not found");
        }

        try
        {
            var pdfBytes = _pdfGenerator.GenerateCertificatePdf(viewModel);
            var fileName = $"Certificate_{viewModel.StudentName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating certificate: {ex.Message}");
        }
    }

    private async Task<CertificateVerificationViewModel?> GetCertificateViewModel(string verificationId)
    {
        if (!verificationId.StartsWith("CERT-"))
        {
            return null;
        }

        var parts = verificationId.Split('-');
        if (parts.Length != 3)
        {
            return null;
        }

        var type = parts[1];
        if (!int.TryParse(parts[2], out int certId))
        {
            return null;
        }

        if (type == "C")
        {
            var courseCert = await _context.CourseCertificates
                .Include(c => c.Student)
                    .ThenInclude(s => s.User)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(c => c.CertificateId == certId);

            if (courseCert == null)
            {
                return null;
            }

            var instructorFirstName = courseCert.Course?.Instructor?.User?.FirstName ?? "Unknown";
            var instructorLastName = courseCert.Course?.Instructor?.User?.LastName ?? "Instructor";
            var instructorFullName = $"{instructorFirstName} {instructorLastName}";

            return new CertificateVerificationViewModel
            {
                IsValid = true,
                VerificationId = verificationId,
                Type = "Course Certificate",
                StudentName = $"{courseCert.Student.User.FirstName} {courseCert.Student.User.LastName}",
                Title = courseCert.Title,
                CourseName = courseCert.Course.Title,
                IssuedDate = courseCert.IssuedDate.ToString("MMMM dd, yyyy"),
                Description = $"This certificate verifies that {courseCert.Student.User.FirstName} {courseCert.Student.User.LastName} has successfully completed the course: {courseCert.Course.Title}",
                InstructorName = instructorFullName
            };
        }
        else if (type == "T")
        {
            var trackCert = await _context.TrackCertificates
                .Include(c => c.Student)
                    .ThenInclude(s => s.User)
                .Include(c => c.Track)
                    .ThenInclude(t => t.TrackCourses)
                        .ThenInclude(tc => tc.Course)
                            .ThenInclude(c => c.Instructor)
                                .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(c => c.CertificateId == certId);

            if (trackCert == null)
            {
                return null;
            }

            var firstInstructor = trackCert.Track?.TrackCourses
                ?.FirstOrDefault()?.Course?.Instructor?.User;
            
            var instructorName = firstInstructor != null 
                ? $"{firstInstructor.FirstName} {firstInstructor.LastName}"
                : "Multiple Instructors";

            return new CertificateVerificationViewModel
            {
                IsValid = true,
                VerificationId = verificationId,
                Type = "Track Certificate",
                StudentName = $"{trackCert.Student.User.FirstName} {trackCert.Student.User.LastName}",
                Title = trackCert.Title,
                CourseName = trackCert.Track.Title,
                IssuedDate = trackCert.IssuedDate.ToString("MMMM dd, yyyy"),
                Description = $"This certificate verifies that {trackCert.Student.User.FirstName} {trackCert.Student.User.LastName} has successfully completed the learning track: {trackCert.Track.Title}",
                InstructorName = instructorName
            };
        }

        return null;
    }
}