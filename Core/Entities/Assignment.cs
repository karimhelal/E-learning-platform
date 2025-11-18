using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

/// <summary>
/// Represents an assignment within a module
/// WEAK ENTITY
/// Will be deleted when parent Module is deleted
/// </summary>
public class Assignment
{
    [Key]
    [Column("assignment_id")]
    [Display(Name = "Assignment ID")]
    public int AssignmentId { get; set; }


    /// <summary>
    /// Foreign Key to parent Module (REQUIRED - Weak Entity)
    /// </summary>
    [Required(ErrorMessage = "{0} is required")]
    [Column("module_id")]
    public int ModuleId { get; set; }


    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Column("title")]
    [Display(Name = "Assignment Title")]
    public string Title { get; set; }

    [Column("instruction")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Instructions")]
    public string? Instruction { get; set; }



    // Navigation Properties
    /// <summary>
    /// Parent Module (REQUIRED - enforces weak entity total participation)
    /// </summary>
    [Required]
    [ForeignKey(nameof(ModuleId))]
    public virtual Module? Module { get; set; }
}