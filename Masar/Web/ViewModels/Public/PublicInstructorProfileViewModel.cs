namespace Web.ViewModels.Public;

public class PublicInstructorProfileViewModel
{
    public PublicInstructorProfileDataViewModel Data { get; set; } = new();
    public string PageTitle { get; set; } = "Instructor Profile";
    
    // For layout - student info if logged in
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? UserInitials { get; set; }
}

public class PublicInstructorProfileDataViewModel
{
    public int InstructorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string Initials { get; set; } = string.Empty;
    
    // Professional Info
    public int? YearsOfExperience { get; set; }
    public string JoinedDate { get; set; } = string.Empty;
    
    // Social Links
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    // Statistics
    public PublicInstructorStatsViewModel Stats { get; set; } = new();
    
    // Skills
    public List<string> Skills { get; set; } = new();
    
    // Published Courses
    public List<PublicInstructorCourseViewModel> Courses { get; set; } = new();
}

public class PublicInstructorStatsViewModel
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class PublicInstructorCourseViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public string Level { get; set; } = string.Empty;
    public string LevelBadgeClass { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryBadgeClass { get; set; } = string.Empty;
    public int StudentsCount { get; set; }
    public float Rating { get; set; }
    public int TotalLessons { get; set; }
    public int DurationHours { get; set; }
    public string GradientStyle { get; set; } = string.Empty;
}