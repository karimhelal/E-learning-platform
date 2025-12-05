using System.ComponentModel.DataAnnotations;
using Core.Entities.Enums;

namespace BLL.DTOs.Course
{
    // Simplified - no longer needs "Draft" since we're publishing directly
    public class CreateCourseDto
    {
        public int InstructorId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        public string? ThumbnailImageUrl { get; set; }
        
        [Required]
        public CourseLevel Level { get; set; }
        
        public int CategoryId { get; set; }
        public int LanguageId { get; set; }
        
        // ADD THESE FOR FULL CREATION
        public List<string> LearningOutcomes { get; set; } = new();
        public List<CreateCourseModuleDto> Modules { get; set; } = new();
    }

    public class CreateCourseModuleDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public List<CreateCourseLessonDto> Lessons { get; set; } = new();
    }

    public class CreateCourseLessonDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public LessonContentType ContentType { get; set; }
        public int Order { get; set; }
        
        // Content fields
        public string? VideoUrl { get; set; }
        public string? PdfUrl { get; set; }
        public int? DurationInSeconds { get; set; }
        
        // Resources
        public List<CreateLessonResourceDto> Resources { get; set; } = new();
    }

    public class CreateLessonResourceDto
    {
        public LessonResourceType ResourceType { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Title { get; set; }
    }

    public class CourseCreationResultDto
    {
        public bool Success { get; set; }
        public int CourseId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}