using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Language
{
    [Key]
    [Display(Name = "Language ID")]
    [Column("language_id")]
    public int LanguageId { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Language Name")]
    [Column("language_name")]
    public string Name { get; set; }

    // TODO: Create a custom validator to check is the passed slug is valid
    // a slug is valid if and only if:
    //      1) contains only lower-case letters
    //      2) hyphens instead of spaces and '_'
    //      3) removes special characters
    //      4) is safe for URLs
    // Examples:
    //      1) /course/python-for-beginners/
    //      2) /course/c-programming-for-absolute-beginners/
    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Language Slug")]
    [Column("language_slug")]
    public string Slug { get; set; }


    public virtual ICollection<LearningEntity_Language>? LearningEntity_Languages { get; set; }
}
