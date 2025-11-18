using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Entities.Enums;

namespace Core.Entities;

/// <summary>
/// Represents a student's enrollment in a course
/// Tracks progress and completion status
/// </summary>
public class CourseEnrollment : EnrollmentBase
{
    [Required]
    [Column("course_id")]
    [Display(Name = "Course ID")]
    public int CourseId { get; set; }



    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    public CourseEnrollment()
    {
        EnrollmentDate = DateTime.Now;
        Status = EnrollmentStatus.Enrolled;
        ProgressPercentage = 0;
    }

    public override LearningEntity GetLearningEntity() => Course;
}