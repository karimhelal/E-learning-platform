using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Instructor;

public class EditCourseFormModel
{
    [Required]
    public string CourseTitle { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string CategoryId { get; set; } = string.Empty;
    
    [Required]
    public string Level { get; set; } = string.Empty;
    
    public string? ThumbnailUrl { get; set; }
    
    public List<string> LearningOutcomes { get; set; } = new();
}