using Core.Entities.Enums;

namespace BLL.DTOs.Student;

public class StudentCourseDetailsDto
{
    // Student Info (ADD THESE TWO PROPERTIES)
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    
    // Course Info
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailImageUrl { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public CourseLevel Level { get; set; }
    
    // Category (from LearningEntity_Category)
    public string CategoryName { get; set; } = string.Empty;
    
    // Instructor Info (from InstructorProfile)
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorInitials { get; set; } = string.Empty;
    
    // Course Stats
    public int TotalModules { get; set; }
    public int TotalLessons { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int TotalStudentsEnrolled { get; set; }
    
    // Student Progress (from CourseEnrollment)
    public decimal ProgressPercentage { get; set; }
    public int CompletedLessons { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public string EnrollmentStatus { get; set; } = "InProgress";
    
    // Track Info (if enrolled via Track)
    public int? TrackId { get; set; }
    public string TrackName { get; set; } = string.Empty;
    
    // Learning Outcomes (from CourseLearningOutcome)
    public List<LearningOutcomeDto> LearningOutcomes { get; set; } = new();
    
    // Modules with Lessons
    public List<ModuleWithProgressDto> Modules { get; set; } = new();
}

public class LearningOutcomeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ModuleWithProgressDto
{
    public int ModuleId { get; set; }
    public int ModuleOrder { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleDescription { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int DurationMinutes { get; set; }
    public int AssignmentsCount { get; set; }
    public decimal ProgressPercentage { get; set; }
    
    public List<LessonWithProgressDto> Lessons { get; set; } = new();
}

public class LessonWithProgressDto
{
    public int LessonId { get; set; }
    public int LessonOrder { get; set; }
    public string LessonName { get; set; } = string.Empty;
    public LessonContentType ContentType { get; set; } // Use your enum
    public int DurationMinutes { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
}