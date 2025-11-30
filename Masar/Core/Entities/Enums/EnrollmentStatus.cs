namespace Core.Entities.Enums;

/// <summary>
/// Defines the status of a student's enrollment in a course
/// </summary>
public enum EnrollmentStatus
{
    NotStarted = 0,
    Enrolled = 1,
    InProgress = 2,
    Completed = 3,
    Dropped = 4
}