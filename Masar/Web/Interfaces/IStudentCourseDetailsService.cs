using BLL.DTOs.Student;

namespace Web.Interfaces;

public interface IStudentCourseDetailsService
{
    Task<StudentCourseDetailsData?> GetCourseDetailsAsync(int userId, int courseId);
    Task<bool> ToggleLessonCompletionAsync(int userId, int lessonId, bool isCompleted);
}

public class StudentCourseDetailsData
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;  // ✅ ADD THIS LINE
    public string InstructorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalModules { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public decimal ProgressPercentage { get; set; }
    public List<ModuleDetailsItem> Modules { get; set; } = new();
}

public class ModuleDetailsItem
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<LessonDetailsItem> Lessons { get; set; } = new();
}

public class LessonDetailsItem
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; }
}