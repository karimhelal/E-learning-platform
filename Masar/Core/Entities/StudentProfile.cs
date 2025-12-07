using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents student-specific profile information
/// One-to-One relationship with User
/// </summary>
public class StudentProfile
{
    [Key]
    [Column("student_id")]
    [Display(Name = "Student ID")]
    public int StudentId { get; set; }


    [Required]
    [Column("user_id")]
    [Display(Name = "User ID")]
    public int UserId { get; set; }

    [Column("bio")]
    [DataType(DataType.MultilineText)]
    [StringLength(1000, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Display(Name = "Biography")]
    public string? Bio { get; set; }

    [Column("location")]
    [StringLength(100)]
    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Column("languages")]
    [StringLength(200)]
    [Display(Name = "Languages")]
    public string? Languages { get; set; }

    [Column("github_url")]
    [DataType(DataType.Url)]
    [Display(Name = "GitHub URL")]
    public string? GithubUrl { get; set; }

    [Column("linkedin_url")]
    [DataType(DataType.Url)]
    [Display(Name = "LinkedIn URL")]
    public string? LinkedInUrl { get; set; }

    [Column("facebook_url")]
    [DataType(DataType.Url)]
    [Display(Name = "Facebook URL")]
    public string? FacebookUrl { get; set; }

    [Column("website_url")]
    [DataType(DataType.Url)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    public virtual ICollection<EnrollmentBase> Enrollments { get; set; }
    public virtual ICollection<CertificateBase> Certificates { get; set; }

    public StudentProfile()
    {
        Certificates = new HashSet<CertificateBase>();
        Enrollments = new HashSet<EnrollmentBase>();
    }
}