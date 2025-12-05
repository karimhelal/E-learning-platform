using Core.Entities;
using Core.Entities.Enums;

namespace BLL.DTOs.Instructor;

public class InstructorCourseEditDto
{
    // Basic Details
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public Category? MainCategory { get; set; }
    public IEnumerable<CourseLearningOutcome>? LearningOutcomes { get; set; } = new List<CourseLearningOutcome>();
    
    // Stats
    public int EnrolledStudents { get; set; }
    public float AverageRating { get; set; }
    public int Completions { get; set; }
    public int AverageProgress { get; set; }
    
    // Modules
    public IEnumerable<ModuleEditDto>? Modules { get; set; } = [];
    
    // Enrolled Students
    public IEnumerable<EnrolledStudentDto>? Students { get; set; } = [];
}

public class ModuleEditDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public int LessonsCount { get; set; }
    public int TotalDurationSeconds { get; set; }
    public IEnumerable<LessonEditDto>? Lessons { get; set; } = [];
}

public class LessonEditDto
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public LessonContentType ContentType { get; set; }
    public int DurationInSeconds { get; set; }
    public string? VideoUrl { get; set; }
    public string? ArticleContent { get; set; }
    public IEnumerable<LessonResourceEditDto>? Resources { get; set; } = []; // ADDED THIS
}

// ADD THIS NEW CLASS
public class LessonResourceEditDto
{
    public int LessonResourceId { get; set; }
    public LessonResourceType ResourceType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public class EnrolledStudentDto
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly? EnrollmentDate { get; set; }
    public float ProgressPercentage { get; set; }
    public DateTime? LastAccessDate { get; set; }
}