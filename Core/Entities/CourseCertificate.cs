using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a course certificate template
/// NOT a weak entity - exists independently
/// Can be reused for multiple courses
/// </summary>
public class CourseCertificate : CertificateBase
{
    [Required(ErrorMessage = "{0} is required")]
    [Column("course_id")]
    [Display(Name = "Course ID")]
    public int CourseId { get; set; }



    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    public override LearningEntity GetLearningEntity() => Course;
}