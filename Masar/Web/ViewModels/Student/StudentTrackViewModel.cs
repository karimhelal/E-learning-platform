namespace Web.ViewModels;

public class StudentTracksViewModel
{
    public StudentTracksData Data { get; set; } = new();
    public string PageTitle { get; set; } = "My Tracks";
}

public class StudentTracksData
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";

    public List<EnrolledTrackViewModel> Tracks { get; set; } = new();

    // Filter counts
    public int TotalCount => Tracks?.Count ?? 0;
    public int InProgressCount => Tracks?.Count(t => t.Status == "in-progress") ?? 0;
    public int CompletedCount => Tracks?.Count(t => t.Status == "completed") ?? 0;
}

public class EnrolledTrackViewModel
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CoursesCount { get; set; }
    public string Duration { get; set; } = string.Empty;  // e.g., "40 Hours"
    public string CertificateText { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int CompletedCourses { get; set; }
    public int CertificatesEarned { get; set; }
    public string Status { get; set; } = "in-progress";  // in-progress/completed
    public string IconClass { get; set; } = "fas fa-route";
    public string IconBackground { get; set; } = "#667eea";
    public string IconColor { get; set; } = "#fff";
    public string StatusIcon { get; set; } = "fas fa-spinner";
    public string StatusText { get; set; } = "In Progress";

    public string ActionUrl { get; set; } = "#";
    public string ActionText { get; set; } = "Continue Learning";

    public List<CoursePreviewViewModel> Courses { get; set; } = new();
}

public class CoursePreviewViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "in-progress";  // completed/in-progress/locked
    public string Icon { get; set; } = "fas fa-play-circle";
}
