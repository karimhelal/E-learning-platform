using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents a track certificate template
/// NOT a weak entity - exists independently
/// Can be reused for multiple tracks
/// </summary>
public class TrackCertificate : CertificateBase
{
    [Required(ErrorMessage = "{0} is required")]
    [Column("track_id")]
    [Display(Name = "Track ID")]
    public int TrackId { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(TrackId))]
    public virtual Track? Track { get; set; }


    public override LearningEntity GetLearningEntity() => Track;
}