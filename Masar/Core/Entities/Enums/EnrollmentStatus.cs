namespace Core.Entities.Enums;

/// <summary>
/// Defines the status of a student's enrollment in a course
/// </summary>
public enum EnrollmentStatus
{
    Enrolled = 0,    // New enrollment, not yet started
    Active = 1,      // Currently learning
    Completed = 2,   // Finished the course
    Inactive = 3,    // Not actively learning
    Dropped = 4,     // Withdrawn from course
    InProgress = 5   // Currently in progress
}