using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public abstract class EnrollmentBase
{
    [Key]
    [Column("enrollment_id")]
    [Display(Name = "Enrollment ID")]
    public int EnrollmentId { get; set; }

    [Required]
    [Column("student_id", Order = 0)]
    [Display(Name = "Student ID")]
    public int StudentId { get; set; }

    [Required]
    [Column("enrollment_date")]
    [Display(Name = "Enrollment Date")]
    [DataType(DataType.DateTime)]
    public DateTime? EnrollmentDate { get; set; }

    [Column("status")]
    [Display(Name = "Enrollment Status")]
    public EnrollmentStatus Status { get; set; }

    [Column("progress_percentage")]
    [Display(Name = "Progress (%)")]
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
    public decimal ProgressPercentage { get; set; }


    [NotMapped]
    public string EnrollmentType => GetType().Name; // "CourseEnrollment" or "TrackEnrollment"




    // Navigation Properties
    [ForeignKey(nameof(StudentId))]
    public virtual StudentProfile? Student { get; set; }


    public abstract LearningEntity GetLearningEntity();
}
