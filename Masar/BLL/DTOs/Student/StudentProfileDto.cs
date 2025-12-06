namespace BLL.DTOs.Student;

public class StudentProfileDto
{
    // Basic Info
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    
    // Learning Info
    public DateTime JoinedDate { get; set; }
    public string? Location { get; set; }
    
    // Social Links
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Statistics
    public int TotalCoursesEnrolled { get; set; }
    public int CompletedCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int CertificatesEarned { get; set; }
    public int TotalLearningHours { get; set; }
    public int CurrentStreak { get; set; }
    
    // Skills (from Skills table)
    public List<string> Skills { get; set; } = new();
    
    // Enrolled Courses
    public List<StudentProfileCourseDto> EnrolledCourses { get; set; } = new();
    
    // Certificates
    public List<StudentCertificateDto> Certificates { get; set; } = new();
}

public class StudentProfileCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}

public class StudentCertificateDto
{
    public int CertificateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string Type { get; set; } = string.Empty;
}