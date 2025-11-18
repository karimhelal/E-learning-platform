using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a learning track (pathway) containing multiple courses
/// Example: "Web Development Track", "Data Science Track"
/// </summary>
public class Track : LearningEntity
{
    [Required(ErrorMessage = "{0} is required")]
    [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Column("title")]
    [Display(Name = "Track Title")]
    public string Title { get; set; }

    [Column("description")]
    [DataType(DataType.MultilineText)]
    [StringLength(2000, ErrorMessage = "{0} cannot exceed {1} characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }



    // Navigation Properties
    public virtual ICollection<Track_Course>? TrackCourses { get; set; }
    public virtual ICollection<Course>? Courses { get; set; }
    public virtual ICollection<TrackEnrollment>? Enrollments { get; set; }
    public virtual ICollection<TrackCertificate>? Certificates { get; set; }

    public Track()
    {
        TrackCourses = new HashSet<Track_Course>();
        Courses = new HashSet<Course>();
        Enrollments = new HashSet<TrackEnrollment>();
        Certificates = new HashSet<TrackCertificate>();
    }
}