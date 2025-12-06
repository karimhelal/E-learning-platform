namespace Web.ViewModels.Instructor;

public class InstructorProfileViewModel
{
    public InstructorProfileDataViewModel Data { get; set; } = new();
    
    // UI-specific properties
    public string PageTitle { get; set; } = "Instructor Profile";
}

public class InstructorProfileDataViewModel
{
    // Basic Info
    public int InstructorId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string Initials { get; set; } = string.Empty;
    
    // Professional Info
    public int? YearsOfExperience { get; set; }
    public string JoinedDate { get; set; } = string.Empty;
    
    // Social Links
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Statistics
    public InstructorProfileStatsViewModel Stats { get; set; } = new();
    
    // Skills (instructor-specific)
    public List<string> Skills { get; set; } = new();
    
    // Courses
    public List<InstructorProfileCourseCardViewModel> Courses { get; set; } = new();
}

public class InstructorProfileStatsViewModel
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TeachingStreak { get; set; }
    public int TotalTeachingHours { get; set; }
    public int StudentInteractions { get; set; }
    public int CertificatesIssued { get; set; }
}

public class InstructorProfileCourseCardViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public float Rating { get; set; }
    public string GradientStyle { get; set; } = string.Empty;
}