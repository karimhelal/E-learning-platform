using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class VideoContent : LessonContent
{
    [Display(Name = "Duration in Seconds")]
    [Column("duration_seconds")]
    public int DurationInSeconds { get; set; }

    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "Video URL")]
    [Column("video_url")]
    [Required]
    public string VideoUrl { get; set; }

    public override LessonContentType ContentType => LessonContentType.Video;
}
