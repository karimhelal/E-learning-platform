using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class UrlResource : LessonResource
{
    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "Link URL")]
    [Column("link_url")]
    [Required]
    public string Link { get; set; }
}
