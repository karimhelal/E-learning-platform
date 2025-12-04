using Core.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class ArticleContent : LessonContent
{
    [Required(ErrorMessage = "Content is required")]
    [Column("article_content")]
    [Display(Name = "Article Body")]
    [DataType(DataType.Html)] // Hint for the View to render a Rich Text Editor
    public string Content { get; set; }


    public override LessonContentType ContentType => LessonContentType.Article;
}
