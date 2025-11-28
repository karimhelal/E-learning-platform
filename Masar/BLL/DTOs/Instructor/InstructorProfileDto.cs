namespace BLL.DTOs.Instructor;

public class InstructorProfileDto
{
    // Basic Info
    public int InstructorId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    
    // Professional Info
    public int? YearsOfExperience { get; set; }
    public string? Location { get; set; }
    public string? Languages { get; set; }
    public DateTime JoinedDate { get; set; }
    
    // Social Links (you may need to add these to the database)
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Statistics
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TeachingStreak { get; set; }
    public int TotalTeachingHours { get; set; }
    public int StudentInteractions { get; set; }
    public int CertificatesIssued { get; set; }
    
    // Skills/Expertise Tags
    public List<string> TeachingExpertise { get; set; } = new();
    
    // Courses
    public List<InstructorProfileCourseDto> Courses { get; set; } = new();
}

public class InstructorProfileCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public float Rating { get; set; }
    public string? ThumbnailImageUrl { get; set; }
}