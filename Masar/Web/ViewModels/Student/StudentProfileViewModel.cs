namespace Web.ViewModels.Student;

public class StudentProfileViewModel
{
    public StudentProfileDataViewModel Data { get; set; } = new();
    public string PageTitle { get; set; } = string.Empty;
}

public class StudentProfileDataViewModel
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty; // For layout compatibility
    public string UserInitials { get; set; } = string.Empty; // For layout compatibility
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Bio { get; set; }
    public string Initials { get; set; } = "JD";
    public string? Location { get; set; }
    public string JoinedDate { get; set; } = string.Empty;
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public StudentProfileStatsViewModel Stats { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<StudentProfileCourseCardViewModel> EnrolledCourses { get; set; } = new();
    public List<StudentCertificateViewModel> Certificates { get; set; } = new();
}

public class StudentProfileStatsViewModel
{
    public int TotalCoursesEnrolled { get; set; }
    public int CompletedCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int CertificatesEarned { get; set; }
    public int TotalLearningHours { get; set; }
    public int CurrentStreak { get; set; }
}

public class StudentProfileCourseCardViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}

public class StudentCertificateViewModel
{
    public int CertificateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IssuedDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}