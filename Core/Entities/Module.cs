using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a module (section) within a course
/// WEAK ENTITY - Cannot exist without a Course
/// Will be deleted when parent Course is deleted
/// </summary>
public class Module
{
    [Key]
    [Column("module_id")]
    [Display(Name = "Module ID")]
    public int ModuleId { get; set; }

    /// <summary>
    /// Foreign Key to parent Course (REQUIRED - Weak Entity)
    /// </summary>
    [Required(ErrorMessage = "Course is required")]
    [Column("course_id")]
    public int CourseId { get; set; }


    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Column("title")]
    [Display(Name = "Module Title")]
    public string Title { get; set; }

    [Column("description")]
    [DataType(DataType.MultilineText)]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Order of module within the course (must be unique per course)
    /// </summary>
    [Required(ErrorMessage = "Order is required")]
    [Column("order")]
    [Display(Name = "Order")]
    [Range(1, 1000, ErrorMessage = "Order must be between 1 and 1000")]
    public int Order { get; set; }



    // Navigation Properties
    /// <summary>
    /// Parent Course (REQUIRED - enforces weak entity total participation)
    /// </summary>
    [Required]
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    public virtual ICollection<Lesson>? Lessons { get; set; }
    public virtual ICollection<Assignment>? Assignments { get; set; }

    public Module()
    {
        Lessons = new HashSet<Lesson>();
        Assignments = new HashSet<Assignment>();
    }
}