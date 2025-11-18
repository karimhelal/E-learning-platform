using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Many-to-Many relationship between Course and Track
/// A course can belong to multiple tracks
/// A track can contain multiple courses
/// </summary>
public class Track_Course
{
    [Key]
    [Column("track_id", Order = 0)]
    [Display(Name = "Track ID")]
    public int TrackId { get; set; }
 
    [Key]
    [Column("course_id", Order = 1)]
    [Display(Name = "Course ID")]
    public int CourseId { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    [ForeignKey(nameof(TrackId))]
    public virtual Track? Track { get; set; }
}