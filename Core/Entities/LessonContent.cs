using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents the actual content of a lesson
/// WEAK ENTITY - Cannot exist without a Lesson
/// One-to-One relationship with Lesson
/// Will be deleted when parent Lesson is deleted
/// </summary>
public abstract class LessonContent
{
    [Key]
    [Column("lesson_content_id")]
    [Display(Name = "Lesson Content ID")]
    public int LessonContentId { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [Column("lesson_id")]
    [Display(Name = "Lesson ID")]
    public int LessonId { get; set; }



    [Column("content")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Content")]
    public string Content { get; set; }


    public abstract LessonContentType ContentType { get; }


    // Navigation Properties
    /// <summary>
    /// Parent Lesson (REQUIRED - enforces weak entity total participation)
    /// </summary>
    [Required]
    [ForeignKey(nameof(LessonId))]
    public virtual Lesson? Lesson { get; set; }
}