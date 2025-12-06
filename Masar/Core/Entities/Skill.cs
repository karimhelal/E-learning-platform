using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class Skill
{
    [Key]
    [Column("skill_id")]
    public int SkillId { get; set; }

    [Required(ErrorMessage = "{0} can't be null or empty")]
    [Column("skill_name")]
    [StringLength(30)]
    public string SkillName { get; set; } = string.Empty;

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// Type of skill: "Student" for learning skills, "Instructor" for teaching skills
    /// </summary>
    [Column("skill_type")]
    [StringLength(20)]
    public string SkillType { get; set; } = "Student";

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
