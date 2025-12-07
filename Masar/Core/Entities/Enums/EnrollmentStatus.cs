namespace Core.Entities.Enums;

/// <summary>
/// Defines the status of a student's enrollment in a course
/// </summary>
public enum EnrollmentStatus
{
    Enrolled = 1,    // New enrollment, not yet started
    InProgress = 2,  // Currently learning
    Completed = 3,   // Finished the course
    Dropped = 4,     // Withdrawn from course
    Active = 5       // Active enrollment (from database)
}