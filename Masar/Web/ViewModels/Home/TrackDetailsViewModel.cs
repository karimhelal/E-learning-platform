namespace Web.ViewModels.Home;

public class TrackDetailsViewModel
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Track Details";
    
    // Category & Level
    public string CategoryName { get; set; } = "Learning Track";
    public string CategoryIcon { get; set; } = "fa-book";
    public string Level { get; set; } = "Beginner";
    public string LevelBadgeClass => Level.ToLower();
    
    // Statistics
    public int DurationHours { get; set; }
    public int CoursesCount { get; set; }
    public int StudentsCount { get; set; }
    public decimal Rating { get; set; } = 4.8m;
    
    // Skills and Courses
    public List<string> Skills { get; set; } = new();
    public List<TrackCourseDetailViewModel> Courses { get; set; } = new();
    
    // Enrollment Status
    public bool IsEnrolled { get; set; }
}

public class TrackCourseDetailViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public string Level { get; set; } = "Beginner";
    public string LevelBadgeClass => Level.ToLower();
    public int DurationHours { get; set; }
    public int ModulesCount { get; set; }
    public int LessonsCount { get; set; }
    public decimal Rating { get; set; } = 4.7m;
}