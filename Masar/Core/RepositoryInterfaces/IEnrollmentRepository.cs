using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface IEnrollmentRepository : IGenericRepository<EnrollmentBase>
{
    // --- 1. Access Control (Security Gates) ---

    /// <summary>
    /// Checks if a student has access to a specific course.
    /// Used by: ClassroomController (Index), CourseController (Details button state)
    /// </summary>
    Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId);

    /// <summary>
    /// Checks if a student has access to a specific lesson.
    /// If the lesson doesn't exist or the student is not enrolled in the course it returns null
    /// Used by: ClassroomController (Index), CourseController (Details button state)
    /// </summary>
    Task<int?> GetCourseIdIfEnrolledAsync(int studentId, int lessonId);

    /// <summary>
    /// Checks if a student has access to a specific track.
    /// </summary>
    Task<bool> IsStudentEnrolledInTrackAsync(int studentId, int trackId);

    // --- 2. Retrieval (Data Fetching) ---

    /// <summary>
    /// Gets the specific enrollment record. Useful if you need the EnrollmentDate or Progress.
    /// </summary>
    Task<CourseEnrollment?> GetCourseEnrollmentAsync(int studentId, int courseId);

    /// <summary>
    /// Gets the specific enrollment record. Useful if you need the EnrollmentDate or Progress.
    /// </summary>
    IQueryable<CourseEnrollment> GetStudentCourseEnrollmentQueryable(int studentId, int courseId);

    /// <summary>
    /// Gets a queryable list of all enrollments for a student.
    /// Returns IQueryable to allow the Service layer to filter (e.g., Active vs Completed) or Paging.
    /// </summary>
    IQueryable<EnrollmentBase> GetStudentEnrollmentsQueryable(int studentId);


    // --- 3. Transactions (Actions) ---

    /// <summary>
    /// Creates a new enrollment (Buying a course).
    /// </summary>
    Task AddEnrollmentAsync(EnrollmentBase enrollment);

    /// <summary>
    /// Updates progress (e.g., 50% -> 55%). 
    /// Note: EF Core tracks changes, so Update isn't always strictly needed if fetching and saving in same scope, 
    /// but good for explicit architecture.
    /// </summary>
    void UpdateEnrollment(EnrollmentBase enrollment);

    /// <summary>
    /// Removes an enrollment (Refunds/Cancellations).
    /// </summary>
    void DeleteEnrollment(EnrollmentBase enrollment);

    Task UpdateProgressPercentageAsync(int studentId, int courseId, decimal progressPercentage);
}
