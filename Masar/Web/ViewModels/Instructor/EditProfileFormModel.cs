using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Instructor;

public class EditProfileFormModel
{
    // Personal Information
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }

    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    public int? YearsOfExperience { get; set; }

    // About / Bio
    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters")]
    public string? Bio { get; set; }

    // Skills (instructor-specific)
    public List<string> Skills { get; set; } = new();

    // Social Links
    [Url(ErrorMessage = "Invalid GitHub URL")]
    public string? GithubUrl { get; set; }

    [Url(ErrorMessage = "Invalid LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [Url(ErrorMessage = "Invalid Facebook URL")]
    public string? FacebookUrl { get; set; }

    [Url(ErrorMessage = "Invalid Website URL")]
    public string? WebsiteUrl { get; set; }
}
