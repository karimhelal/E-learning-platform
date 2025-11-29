namespace Web.Interfaces;

/// <summary>
/// Simplified interface for Student Dashboard
/// </summary>
public interface IStudentDashboardService
{
    Task<StudentDashboardData?> GetDashboardDataAsync(int userId);
}

public class StudentDashboardData
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";

    // Stats
    public DashboardStats Stats { get; set; } = new();
    
    // Continue Learning Courses
    public List<ContinueLearningCourse> ContinueLearningCourses { get; set; } = new();
    
    // My Tracks
    public List<EnrolledTrack> EnrolledTracks { get; set; } = new();
}

public class DashboardStats
{
    public int ActiveTracks { get; set; }
    public int EnrolledCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CertificatesEarned { get; set; }
}

public class ContinueLearningCourse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "fa-laptop-code";
    public string CategoryBadgeClass { get; set; } = "badge-purple";
    public string InstructorName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public int DurationHours { get; set; }
}

public class EnrolledTrack
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CoursesCount { get; set; }
    public int TotalHours { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string IconClass { get; set; } = "fa-laptop-code";
    public string IconStyle { get; set; } = "";
}