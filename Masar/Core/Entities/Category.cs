using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Category
{
    [Key]
    [Display(Name = "Learning Entity Category ID")]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Category Name")]
    [Column("category_name")]
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
    [Display(Name = "Category Slug")]
    [Column("category_slug")]
    public string Slug { get; set; }

    [Display(Name = "Category's Parent Category ID")]
    [Column("parent_category_id")]
    public int? ParentCategoryId { get; set; }

    
    [ForeignKey(nameof(ParentCategoryId))]
    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category>? SubCategories { get; set; }
    public virtual ICollection<LearningEntity_Category>? LearningEntity_Categories { get; set; }
}
