using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class VideoContent : LessonContent
{
    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "PDF URL")]
    [Column("pdf_url")]
    [Required]
    public string VideoUrl { get; set; }

    [Display(Name = "Duration in Seconds")]
    [Column("duration_seconds")]
    public int DurationInSeconds { get; set; }

    public override LessonContentType ContentType => LessonContentType.Video;
}
