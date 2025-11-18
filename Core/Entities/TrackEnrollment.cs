using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Many-to-Many relationship between User and Track
/// Tracks which learning paths a user is enrolled in
/// </summary>
public class TrackEnrollment : EnrollmentBase
{
    [Required]
    [Column("track_id", Order = 1)]
    [Display(Name = "Track ID")]
    public int TrackId { get; set; }



    // Navigation Properties
    [ForeignKey(nameof(TrackId))]
    public virtual Track? Track { get; set; }

    public TrackEnrollment()
    {
        EnrollmentDate = DateTime.Now;
        Status = EnrollmentStatus.Enrolled;
        ProgressPercentage = 0;
    }

    public override LearningEntity GetLearningEntity() => Track;
}