using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class PdfResource : LessonResource
{
    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "PDF URL")]
    [Column("pdf_url")]
    [Required]
    public string PdfUrl { get; set; }
}
