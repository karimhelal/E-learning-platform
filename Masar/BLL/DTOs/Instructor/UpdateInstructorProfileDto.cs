namespace BLL.DTOs.Instructor;

public class UpdateInstructorProfileDto
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int? YearsOfExperience { get; set; }

    // About / Bio
    public string? Bio { get; set; }

    // Skills (instructor-specific, stored with SkillType = "Instructor")
    public List<string> Skills { get; set; } = new();

    // Social Links
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
}