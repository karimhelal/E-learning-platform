using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a resource of a lesson
/// WEAK ENTITY - Cannot exist without a Lesson
/// One-to-One relationship with Lesson
/// Will be deleted when parent Lesson is deleted
/// </summary>
public abstract class LessonResource
{
    [Key]
    [Column("lesson_resource_id")]
    [Display(Name = "Lesson Resource ID")]
    public int LessonResourceId { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [Column("lesson_id")]
    [Display(Name = "Lesson ID")]
    public int LessonId { get; set; }


    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "Resource URL")]
    [Column("resource_url")]
    [Required]
    public string Url { get; set; }

    
    [StringLength(50)]
    [Display(Name = "Resource Title")]
    [Column("resource_title")]
    public string Title { get; set; }


    [NotMapped]
    public LessonResourceType ResourceKind =>
        this switch
        {
            PdfResource => LessonResourceType.PDF,
            ZipResource => LessonResourceType.ZIP,
            UrlResource => LessonResourceType.URL,
            _ => throw new Exception("Unknown resource type")
        };


    // Navigation Properties
    /// <summary>
    /// Parent Lesson (REQUIRED - enforces weak entity total participation)
    /// </summary>
    [Required]
    [ForeignKey(nameof(LessonId))]
    public virtual Lesson? Lesson { get; set; }
}
