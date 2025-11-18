using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// WEAK Entity
/// Tracks a Lesson progress for the User (student) at a specific Course
/// Composite Primary Key: (UserId, CourseId, LessonId)
/// </summary>
public class LessonProgress
{
    [Key]
    [Column("lesson_progress_id")]
    [Display(Name = "Lesson Progress ID")]
    public int LessonProgressId { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Student ID")]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Lesson ID")]
    [Column("lesson_id")]
    public int LessonId { get; set; }


    [Column("started_date")]
    [Display(Name = "Started Date")]
    [DataType(DataType.DateTime)]
    public DateTime? StartedDate { get; set; }

    [Column("completed_date")]
    [Display(Name = "Completed Date")]
    [DataType(DataType.DateTime)]
    public DateTime? CompletedDate { get; set; }

    [Column("is_completed")]
    [Display(Name = "Completed")]
    public bool IsCompleted { get; set; }



    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual StudentProfile? Student { get; set; }

    [ForeignKey(nameof(LessonId))]
    public virtual Lesson? Lesson { get; set; }

    public LessonProgress()
    {
        IsCompleted = false;
    }
}