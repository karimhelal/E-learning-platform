using Core.Entities;
using Core.Entities.Enums;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;

namespace Web.Services;

public class CertificateGenerationService : ICertificateGenerationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CertificateGenerationService> _logger;

    public CertificateGenerationService(
        AppDbContext context,
        ILogger<CertificateGenerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> GenerateMissingCertificatesAsync(int studentId)
    {
        var certificatesGenerated = 0;

        try
        {
            // Get all completed course enrollments
            var completedCourseEnrollments = await _context.CourseEnrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId && 
                           (e.Status == EnrollmentStatus.Completed || e.ProgressPercentage >= 100))
                .ToListAsync();

            // Get existing course certificates
            var existingCourseCertificates = await _context.CourseCertificates
                .Where(c => c.StudentId == studentId)
                .Select(c => c.CourseId)
                .ToListAsync();

            // Generate certificates for completed courses that don't have one
            foreach (var enrollment in completedCourseEnrollments)
            {
                if (!existingCourseCertificates.Contains(enrollment.CourseId))
                {
                    var generated = await GenerateCourseCertificateAsync(studentId, enrollment.CourseId);
                    if (generated)
                    {
                        certificatesGenerated++;
                        _logger.LogInformation(
                            "Generated certificate for Student {StudentId}, Course {CourseId}", 
                            studentId, enrollment.CourseId);
                    }
                }
            }

            // Get all completed track enrollments
            var completedTrackEnrollments = await _context.TrackEnrollments
                .Include(e => e.Track)
                    .ThenInclude(t => t.TrackCourses)
                .Where(e => e.StudentId == studentId && 
                           (e.Status == EnrollmentStatus.Completed || e.ProgressPercentage >= 100))
                .ToListAsync();

            // Get existing track certificates
            var existingTrackCertificates = await _context.TrackCertificates
                .Where(c => c.StudentId == studentId)
                .Select(c => c.TrackId)
                .ToListAsync();

            // Generate certificates for completed tracks that don't have one
            foreach (var enrollment in completedTrackEnrollments)
            {
                if (!existingTrackCertificates.Contains(enrollment.TrackId))
                {
                    var generated = await GenerateTrackCertificateAsync(studentId, enrollment.TrackId);
                    if (generated)
                    {
                        certificatesGenerated++;
                        _logger.LogInformation(
                            "Generated certificate for Student {StudentId}, Track {TrackId}", 
                            studentId, enrollment.TrackId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating missing certificates for Student {StudentId}", studentId);
        }

        return certificatesGenerated;
    }

    public async Task<bool> GenerateCourseCertificateAsync(int studentId, int courseId)
    {
        try
        {
            // Check if certificate already exists
            var exists = await _context.CourseCertificates
                .AnyAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            if (exists)
            {
                _logger.LogInformation(
                    "Certificate already exists for Student {StudentId}, Course {CourseId}", 
                    studentId, courseId);
                return false;
            }

            // Verify the course is completed
            var enrollment = await _context.CourseEnrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
            {
                _logger.LogWarning(
                    "No enrollment found for Student {StudentId}, Course {CourseId}", 
                    studentId, courseId);
                return false;
            }

            if (enrollment.ProgressPercentage < 100 && enrollment.Status != EnrollmentStatus.Completed)
            {
                _logger.LogWarning(
                    "Course not completed. Student {StudentId}, Course {CourseId}, Progress {Progress}%", 
                    studentId, courseId, enrollment.ProgressPercentage);
                return false;
            }

            // Get course details for the certificate title
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return false;

            // Create the certificate
            var certificate = new CourseCertificate
            {
                StudentId = studentId,
                CourseId = courseId,
                Title = $"Certificate of Completion - {course.Title}",
                IssuedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Link = string.Empty // Will be generated on-demand via verification URL
            };

            await _context.CourseCertificates.AddAsync(certificate);
            
            // Update enrollment status to Completed if not already
            if (enrollment.Status != EnrollmentStatus.Completed)
            {
                enrollment.Status = EnrollmentStatus.Completed;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully created certificate {CertificateId} for Student {StudentId}, Course {CourseId}", 
                certificate.CertificateId, studentId, courseId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error generating course certificate for Student {StudentId}, Course {CourseId}", 
                studentId, courseId);
            return false;
        }
    }

    public async Task<bool> GenerateTrackCertificateAsync(int studentId, int trackId)
    {
        try
        {
            // Check if certificate already exists
            var exists = await _context.TrackCertificates
                .AnyAsync(c => c.StudentId == studentId && c.TrackId == trackId);

            if (exists)
            {
                _logger.LogInformation(
                    "Certificate already exists for Student {StudentId}, Track {TrackId}", 
                    studentId, trackId);
                return false;
            }

            // Verify all courses in the track are completed
            var track = await _context.Tracks
                .Include(t => t.TrackCourses)
                .FirstOrDefaultAsync(t => t.Id == trackId);

            if (track == null)
                return false;

            var courseIds = track.TrackCourses?.Select(tc => tc.CourseId).ToList() ?? new List<int>();

            if (!courseIds.Any())
            {
                _logger.LogWarning("Track {TrackId} has no courses", trackId);
                return false;
            }

            // Check if all courses are completed
            var completedCourses = await _context.CourseEnrollments
                .Where(e => e.StudentId == studentId && 
                           courseIds.Contains(e.CourseId) &&
                           (e.Status == EnrollmentStatus.Completed || e.ProgressPercentage >= 100))
                .CountAsync();

            if (completedCourses < courseIds.Count)
            {
                _logger.LogWarning(
                    "Not all courses completed. Student {StudentId}, Track {TrackId}, Completed {Completed}/{Total}", 
                    studentId, trackId, completedCourses, courseIds.Count);
                return false;
            }

            // Create the track certificate
            var certificate = new TrackCertificate
            {
                StudentId = studentId,
                TrackId = trackId,
                Title = $"Certificate of Completion - {track.Title} Learning Track",
                IssuedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Link = string.Empty
            };

            await _context.TrackCertificates.AddAsync(certificate);

            // Update track enrollment status
            var trackEnrollment = await _context.TrackEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrackId == trackId);

            if (trackEnrollment != null && trackEnrollment.Status != EnrollmentStatus.Completed)
            {
                trackEnrollment.Status = EnrollmentStatus.Completed;
                trackEnrollment.ProgressPercentage = 100;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully created certificate {CertificateId} for Student {StudentId}, Track {TrackId}", 
                certificate.CertificateId, studentId, trackId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error generating track certificate for Student {StudentId}, Track {TrackId}", 
                studentId, trackId);
            return false;
        }
    }
}