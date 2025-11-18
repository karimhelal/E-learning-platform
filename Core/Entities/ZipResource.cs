using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class ZipResource : LessonResource
{
    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "ZIP URL")]
    [Column("zip_url")]
    [Required]
    public string ZipUrl { get; set; }
}
