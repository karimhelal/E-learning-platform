using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a course in the e-learning platform
/// Taught by an Instructor and contains Modules
/// </summary>
public class Course : LearningEntity
{
    [Required(ErrorMessage = "{0} is required")]
    [Column("instructor_id")]
    [Display(Name = "Instructor ID")]
    public int InstructorId { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("title")]
    [Display(Name = "Course Title")]
    public string Title { get; set; }

    [Column("description")]
    [DataType(DataType.MultilineText)]
    [StringLength(2000, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "Thumbnail Image URL")]
    [Column("thumbnail_image_url")]
    public string? ThumbnailImageUrl { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Column("language")]
    [Display(Name = "Course Language")]
    public string Language { get; set; }

    [Column("difficulty_level")]
    [Display(Name = "Difficulty Level")]
    public CourseLevel Level { get; set; } = CourseLevel.Undefined;




    // Navigation Properties
    [ForeignKey(nameof(InstructorId))]
    public virtual InstructorProfile? Instructor { get; set; }

    public virtual ICollection<Module>? Modules { get; set; }
    public virtual ICollection<Track_Course>? CourseTracks { get; set; }
    public virtual ICollection<Track>? Tracks { get; set; }
    public virtual ICollection<CourseEnrollment>? Enrollments { get; set; }
    public virtual ICollection<CourseCertificate>? Certificates { get; set; }

    public Course()
    {
        Modules = new HashSet<Module>();
        Tracks = new HashSet<Track>();
        CourseTracks = new HashSet<Track_Course>();
        Enrollments = new HashSet<CourseEnrollment>();
        Certificates = new HashSet<CourseCertificate>();
    }
}