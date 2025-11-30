using Web.Interfaces;

namespace Web.ViewModels.Student
{
    public class StudentBrowseTracksViewModel
    {
        public StudentBrowseTracksPageData Data { get; set; } = new();
    }

    /// <summary>
    /// Root object for Browse Tracks page
    /// </summary>
    public class StudentBrowseTracksPageData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string UserInitials { get; set; } = "JD";

        public List<BrowseTrackItem> Tracks { get; set; } = new();
        public BrowseTrackPageStats Stats { get; set; } = new();
    }

    /// <summary>
    /// Stats at top of page
    /// </summary>
    public class BrowseTrackPageStats
    {
        public int TotalTracks { get; set; }
        public int BeginnerTracks { get; set; }
        public int IntermediateTracks { get; set; }
        public int AdvancedTracks { get; set; }
    }

    /// <summary>
    /// A single track item for browsing
    /// </summary>
    public class BrowseTrackItem
    {
        public int TrackId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Track metadata
        public string Level { get; set; } = "Beginner";
        public string CategoryName { get; set; } = "Learning Track";
        public string CategoryIcon { get; set; } = "fa-laptop-code";
        public string LevelBadgeClass { get; set; } = "beginner";

        // Statistics
        public int CoursesCount { get; set; }
        public int DurationHours { get; set; }
        public int StudentsCount { get; set; }
        public decimal Rating { get; set; } = 4.8m;

        // Skills and content
        public List<string> Skills { get; set; } = new();
        public List<CoursePreview> CoursesPreview { get; set; } = new();

        // Action
        public string ActionText { get; set; } = "Enroll Now";
        public string ActionUrl { get; set; } = "#";
    }

    /// <summary>
    /// Course preview inside a track
    /// </summary>
    public class CoursePreview
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Intermediate";
    }
}
