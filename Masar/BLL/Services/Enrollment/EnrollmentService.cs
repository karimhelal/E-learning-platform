using BLL.Interfaces.Enrollment;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Enrollment;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly AppDbContext _context;

    public EnrollmentService(IEnrollmentRepository enrollmentRepo, AppDbContext context)
    {
        _enrollmentRepo = enrollmentRepo;
        _context = context;
    }

    public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
    {
        return await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId, courseId);
    }

    public async Task<bool> IsStudentEnrolledInTrackAsync(int studentId, int trackId)
    {
        return await _enrollmentRepo.IsStudentEnrolledInTrackAsync(studentId, trackId);
    }

    public async Task<int?> GetCourseIdIfEnrolled(int studentId, int lessonId)
    {
        return await _enrollmentRepo.GetCourseIdIfEnrolledAsync(studentId, lessonId);
    }

    public async Task<EnrollmentResult> EnrollInCourseAsync(int studentId, int courseId)
    {
        try
        {
            // Check if already enrolled
            var alreadyEnrolled = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId, courseId);
            if (alreadyEnrolled)
            {
                return new EnrollmentResult
                {
                    Success = true,
                    Message = "You are already enrolled in this course",
                    RedirectUrl = $"/student/course/details/{courseId}"
                };
            }

            // Verify course exists and is published
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Status == LearningEntityStatus.Published);

            if (course == null)
            {
                return new EnrollmentResult
                {
                    Success = false,
                    Message = "Course not found or not available for enrollment"
                };
            }

            // Verify student exists
            var studentExists = await _context.StudentProfiles.AnyAsync(s => s.StudentId == studentId);
            if (!studentExists)
            {
                return new EnrollmentResult
                {
                    Success = false,
                    Message = "Student profile not found"
                };
            }

            // Create enrollment
            var enrollment = new CourseEnrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled,
                ProgressPercentage = 0
            };

            await _context.CourseEnrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return new EnrollmentResult
            {
                Success = true,
                EnrollmentId = enrollment.EnrollmentId,
                Message = "Successfully enrolled in the course!",
                RedirectUrl = $"/student/course/details/{courseId}"
            };
        }
        catch (Exception ex)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            };
        }
    }

    public async Task<EnrollmentResult> EnrollInTrackAsync(int studentId, int trackId)
    {
        try
        {
            // Check if already enrolled in track
            var alreadyEnrolled = await _enrollmentRepo.IsStudentEnrolledInTrackAsync(studentId, trackId);
            if (alreadyEnrolled)
            {
                return new EnrollmentResult
                {
                    Success = true,
                    Message = "You are already enrolled in this track",
                    RedirectUrl = $"/student/track/{trackId}"
                };
            }

            // Verify track exists and is published
            var track = await _context.Tracks
                .Include(t => t.TrackCourses)
                    .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(t => t.Id == trackId && t.Status == LearningEntityStatus.Published);

            if (track == null)
            {
                return new EnrollmentResult
                {
                    Success = false,
                    Message = "Track not found or not available for enrollment"
                };
            }

            // Verify student exists
            var studentExists = await _context.StudentProfiles.AnyAsync(s => s.StudentId == studentId);
            if (!studentExists)
            {
                return new EnrollmentResult
                {
                    Success = false,
                    Message = "Student profile not found"
                };
            }

            // Create track enrollment
            var trackEnrollment = new TrackEnrollment
            {
                StudentId = studentId,
                TrackId = trackId,
                EnrollmentDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled,
                ProgressPercentage = 0
            };

            await _context.TrackEnrollments.AddAsync(trackEnrollment);

            // Also enroll in all courses in the track
            var courseIds = track.TrackCourses?
                .Where(tc => tc.Course != null && tc.Course.Status == LearningEntityStatus.Published)
                .Select(tc => tc.CourseId)
                .ToList() ?? new List<int>();

            foreach (var courseId in courseIds)
            {
                var courseEnrolled = await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId, courseId);
                if (!courseEnrolled)
                {
                    var courseEnrollment = new CourseEnrollment
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        EnrollmentDate = DateTime.UtcNow,
                        Status = EnrollmentStatus.Enrolled,
                        ProgressPercentage = 0
                    };
                    await _context.CourseEnrollments.AddAsync(courseEnrollment);
                }
            }

            await _context.SaveChangesAsync();

            return new EnrollmentResult
            {
                Success = true,
                EnrollmentId = trackEnrollment.EnrollmentId,
                Message = "Successfully enrolled in the track!",
                RedirectUrl = $"/student/track/{trackId}"
            };
        }
        catch (Exception ex)
        {
            return new EnrollmentResult
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            };
        }
    }

    public async Task<bool> UnenrollFromCourseAsync(int studentId, int courseId)
    {
        try
        {
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
                return false;

            _context.CourseEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnenrollFromTrackAsync(int studentId, int trackId)
    {
        try
        {
            var enrollment = await _context.TrackEnrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrackId == trackId);

            if (enrollment == null)
                return false;

            _context.TrackEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
