using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Many-to-Many relationship between Course and Track
/// A course can belong to multiple tracks
/// A track can contain multiple courses
/// </summary>
public class LearningEntity_Category
{
    [Column("learning_entity_id", Order = 0)]
    [Display(Name = "Learning Entity ID")]
    public int LearningEntityId { get; set; }

    [Column("category_id", Order = 1)]
    [Display(Name = "Category ID")]
    public int CategoryId { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(LearningEntityId))]
    public virtual LearningEntity? LearningEntity { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }
}
