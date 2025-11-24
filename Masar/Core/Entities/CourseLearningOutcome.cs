using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class CourseLearningOutcome
{
    [Column("course_outcome_id")]
    [Display(Name = "Course Learning Outcome ID")]
    public int Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [Column("title")]
    [Display(Name = "Outcome Title")]
    public string Title { get; set; }

    [Column("description")]
    [Display(Name = "Outcome Discription")]
    public string Description { get; set; }


    [Required(ErrorMessage = "{0} is required")]
    [Display(Name = "Course ID")]
    [Column("course_id")]
    public int CourseId { get; set; }


    // Navigation Property
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }
}
