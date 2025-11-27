namespace Web.Interfaces;

/// <summary>
/// Service interface for Student My Courses page
/// </summary>
public interface IStudentCoursesService
{
    Task<StudentCoursesData?> GetMyCoursesAsync(int studentId);
}

public class StudentCoursesData
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD"; // ADD THIS LINE
    
    public List<MyCourseItem> AllCourses { get; set; } = new();
    public List<MyCourseItem> InProgressCourses { get; set; } = new();
    public List<MyCourseItem> CompletedCourses { get; set; } = new();
    
    public int AllCoursesCount => AllCourses.Count;
    public int InProgressCount => InProgressCourses.Count;
    public int CompletedCount => CompletedCourses.Count;
}

public class MyCourseItem
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "fa-laptop-code";
    public string CategoryBadgeClass { get; set; } = "badge-purple";
    public string InstructorName { get; set; } = string.Empty;
    public int ModulesCount { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int DurationHours { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string Status { get; set; } = "InProgress";
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}