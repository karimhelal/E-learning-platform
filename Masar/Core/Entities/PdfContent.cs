using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class PdfContent : LessonContent
{
    [Url(ErrorMessage = "{0} should be a valid URL")]
    [Display(Name = "PDF URL")]
    [Column("pdf_url")]
    [Required]
    public string PdfUrl { get; set; }

    public override LessonContentType ContentType => LessonContentType.Article;
}
