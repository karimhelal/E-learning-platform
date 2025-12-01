using Core.Entities.Enums;

namespace BLL.DTOs.Instructor;

public class UpdateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public int CategoryId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<string> LearningOutcomes { get; set; } = new();
}

public class UpdateModuleDto
{
    public int? ModuleId { get; set; } // Null for new modules
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
}

public class UpdateLessonDto
{
    public int? LessonId { get; set; } // Null for new lessons
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public LessonContentType ContentType { get; set; }
    public int Order { get; set; }
    public string? VideoUrl { get; set; }
    public string? PdfUrl { get; set; }
    public int? DurationInSeconds { get; set; }
    
    // ADD THIS PROPERTY
    public List<UpdateLessonResourceDto> Resources { get; set; } = new();
}

// ADD THIS NEW CLASS
public class UpdateLessonResourceDto
{
    public int? LessonResourceId { get; set; }  // Null for new resources
    public LessonResourceType ResourceType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}