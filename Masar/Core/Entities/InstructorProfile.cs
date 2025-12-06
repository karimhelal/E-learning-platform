using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents instructor-specific profile information
/// One-to-One relationship with User
/// </summary>
public class InstructorProfile
{
    [Key]
    [Column("instructor_id")]
    [Display(Name = "Instructor ID")]
    public int InstructorId { get; set; }


    [Required]
    [Column("user_id")]
    [Display(Name = "User ID")]
    public int UserId { get; set; }


    [Column("bio")]
    [DataType(DataType.MultilineText)]
    [StringLength(1000, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Display(Name = "Biography")]
    public string? Bio { get; set; }

    [Column("years_of_experience")]
    [Display(Name = "Years of Experience")]
    [Range(0, 50, ErrorMessage = "{0} of experience must be between {1} and {2}")]
    public int? YearsOfExperience { get; set; }

    // Social Links - NotMapped until migration is run
    [NotMapped]
    [DataType(DataType.Url)]
    [Display(Name = "GitHub URL")]
    public string? GithubUrl { get; set; }

    [NotMapped]
    [DataType(DataType.Url)]
    [Display(Name = "LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [NotMapped]
    [DataType(DataType.Url)]
    [Display(Name = "Facebook URL")]
    public string? FacebookUrl { get; set; }

    [NotMapped]
    [DataType(DataType.Url)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public virtual ICollection<Course>? OwnedCourses { get; set; }

    public InstructorProfile()
    {
        OwnedCourses = new HashSet<Course>();
    }
}