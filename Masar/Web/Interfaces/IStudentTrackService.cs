namespace Web.Interfaces;

/// <summary>
/// Service for retrieving Track page data (My Tracks)
/// </summary>
public interface IStudentTrackService
{
    Task<StudentTracksData?> GetStudentTracksAsync(int studentId);
}

/// <summary>
/// Root object returned to the View
/// </summary>
public class StudentTracksData
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";

    // Stats
    public TrackPageStats Stats { get; set; } = new();

    // All tracks for student
    public List<StudentTrackItem> Tracks { get; set; } = new();
}

/// <summary>
/// Stats at top of page
/// </summary>
public class TrackPageStats
{
    public int TotalTracks { get; set; }
    public int InProgressTracks { get; set; }
    public int CompletedTracks { get; set; }
}

/// <summary>
/// A single track box on the page
/// </summary>
public class StudentTrackItem
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int CoursesCount { get; set; }
    public decimal ProgressPercentage { get; set; }

    public string Status { get; set; } = "in-progress"; // or completed

    public string IconClass { get; set; } = "fa-laptop-code";
    public string ActionText { get; set; } = "Continue Learning";
    public string ActionUrl { get; set; } = "#";

    // List of courses inside this track
    public List<TrackCoursePreview> Courses { get; set; } = new();
}

/// <summary>
/// Small preview card inside track card
/// </summary>
public class TrackCoursePreview
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-book";
    public string Status { get; set; } = "completed"; // or in-progress
}
