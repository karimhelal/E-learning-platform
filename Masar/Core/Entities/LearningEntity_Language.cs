using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Many-to-Many relationship between LearningEntity and Language
/// A LearningEntity can contain multiple Languages
/// A Language can be in more than one LearningEntity
/// </summary>
public class LearningEntity_Language
{
    [Column("learning_entity_id", Order = 0)]
    [Display(Name = "Learning Entity ID")]
    public int LearningEntityId { get; set; }

    [Column("language_id", Order = 1)]
    [Display(Name = "Language ID")]
    public int LanguageId { get; set; }


    // Navigation Properties
    [ForeignKey(nameof(LearningEntityId))]
    public virtual LearningEntity? LearningEntity { get; set; }

    [ForeignKey(nameof(LanguageId))]
    public virtual Language? Language { get; set; }
}
