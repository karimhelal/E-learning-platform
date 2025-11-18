using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public abstract class LearningEntity
{
    [Key]
    [Column("id")]
    [Display(Name = "Learning Entity ID")]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Created Date")]
    [Column("created_date")]
    public DateOnly CreatedDate { get; set; }

    [NotMapped]
    public string LearningEntityType => GetType().Name; // "Course" or "Track"


    public virtual ICollection<LearningEntity_Category>? LearningEntity_Categories { get; set; }
    public virtual ICollection<Category>? Categories { get; set; }
}
