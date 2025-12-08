namespace BLL.Interfaces.Enrollment;

public interface IEnrollmentService
{
    // Check enrollment status
    Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
    Task<bool> IsStudentEnrolledInTrackAsync(int studentId, int trackId);
    Task<int?> GetCourseIdIfEnrolled(int studentId, int lessonId);
    
    // Enroll in course/track
    Task<EnrollmentResult> EnrollInCourseAsync(int studentId, int courseId);
    Task<EnrollmentResult> EnrollInTrackAsync(int studentId, int trackId);
    
    // Unenroll (optional)
    Task<bool> UnenrollFromCourseAsync(int studentId, int courseId);
    Task<bool> UnenrollFromTrackAsync(int studentId, int trackId);
}

public class EnrollmentResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? EnrollmentId { get; set; }
    public string? RedirectUrl { get; set; }
}
