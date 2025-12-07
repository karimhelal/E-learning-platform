namespace BLL.DTOs.Instructor;

/// <summary>
/// DTO for displaying instructor profile publicly to students
/// </summary>
public class PublicInstructorProfileDto
{
    public int InstructorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    
    // Professional Info
    public int? YearsOfExperience { get; set; }
    public DateTime JoinedDate { get; set; }
    
    // Social Links (public only)
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Statistics
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Skills
    public List<string> Skills { get; set; } = new();
    
    // Published Courses only
    public List<PublicInstructorCourseDto> Courses { get; set; } = new();
}

public class PublicInstructorCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public string Level { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public float Rating { get; set; }
    public int TotalLessons { get; set; }
    public int DurationHours { get; set; }
}