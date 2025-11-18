using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a lesson within a module
/// WEAK ENTITY - Cannot exist without a Module
/// Will be deleted when parent Module is deleted
/// </summary>
public class Lesson
{
    [Key]
    [Column("lesson_id")]
    [Display(Name = "Lesson ID")]
    public int LessonId { get; set; }



    /// <summary>
    /// Foreign Key to parent Module (REQUIRED - Weak Entity)
    /// </summary>
    [Required(ErrorMessage = "{0} is required")]
    [Column("module_id")]
    [Display(Name = "Module ID")]
    public int ModuleId { get; set; }

    //[Required(ErrorMessage = " {0} is required")]
    //[Column("lesson_content_id")]
    //[Display(Name = "Lesson Content ID")]
    //public int LessonContentId { get; set; }



    [Required(ErrorMessage = "{0} is required")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("title")]
    [Display(Name = "Lesson Title")]
    public string Title { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Column("content_type")]
    [Display(Name = "Content Type")]
    public LessonContentType ContentType { get; set; }

    /// <summary>
    /// Order of lesson within the module (must be unique per module)
    /// </summary>
    [Required(ErrorMessage = "{0} is required")]
    [Column("order")]
    [Display(Name = "Order")]
    [Range(1, 1000, ErrorMessage = "{0} must be between {1} and {2}")]
    public int Order { get; set; }





    /// <summary>
    /// Parent Module (REQUIRED - enforces weak entity total participation)
    /// </summary>
    [Required]
    [ForeignKey(nameof(ModuleId))]
    public virtual Module? Module { get; set; }


    public virtual LessonContent? LessonContent { get; set; }
    public virtual ICollection<LessonProgress>? LessonProgresses { get; set; }
    public virtual ICollection<LessonResource>? LessonResources { get; set; }
}