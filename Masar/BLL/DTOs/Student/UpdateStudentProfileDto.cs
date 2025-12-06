namespace BLL.DTOs.Student;

public class UpdateStudentProfileDto
{
    // Profile Images
    public string? ProfilePictureUrl { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Location { get; set; }

    // About / Bio
    public string? Bio { get; set; }

    // Skills (saved as Skills in database)
    public List<string> Skills { get; set; } = new();

    // Social Links
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
}
