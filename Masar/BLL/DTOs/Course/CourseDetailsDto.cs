using Core.Entities;
using Core.Entities.Enums;

namespace BLL.DTOs.Course;

public class CourseDetailsDto
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public CourseLevel Level { get; set; }
    public DateOnly CreatedDate { get; set; }
    
    // Instructor Info
    public string InstructorName { get; set; }
    public string InstructorBio { get; set; }
    public int InstructorYearsOfExperience { get; set; }
    
    // Categories & Languages
    public Category MainCategory { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public IEnumerable<Language> Languages { get; set; }
    
    // Learning Outcomes
    public IEnumerable<CourseLearningOutcome> LearningOutcomes { get; set; }
    
    // Modules & Lessons
    public IEnumerable<ModuleWithLessonsDto> Modules { get; set; }
    
    // Statistics
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
    public int TotalModules { get; set; }
    public int TotalDurationMinutes { get; set; }
    public float AverageRating { get; set; }
    public int NumberOfReviews { get; set; }
    
    // Computed Properties
    public int TotalDurationHours => TotalDurationMinutes / 60;
    public string FormattedDuration => $"{TotalDurationHours}h {TotalDurationMinutes % 60}m";
}

public class ModuleWithLessonsDto
{
    public int ModuleId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
    public IEnumerable<LessonSummaryDto> Lessons { get; set; }
}

public class LessonSummaryDto
{
    public int LessonId { get; set; }
    public string Title { get; set; }
    public int Order { get; set; }
    public LessonContentType ContentType { get; set; }
    public int DurationSeconds { get; set; }
    public string FormattedDuration => $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}";
}