using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Student;

public class EditStudentProfileFormModel
{
    // Profile Images
    public string? ProfilePictureUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    // Personal Information
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    public string? Location { get; set; }

    // About / Bio
    [StringLength(2000, ErrorMessage = "Bio cannot exceed 2000 characters")]
    public string? Bio { get; set; }

    // Skills - Dynamic list of strings (saved as Skills in database)
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
