using Web.ViewModels.Misc;

namespace Web.ViewModels.Home;

public class HomeBrowseTracksViewModel
{
    public string PageTitle { get; set; } = "Browse Learning Tracks";
    public List<HomeTrackCardViewModel> Tracks { get; set; } = new();
    public PaginationSettingsViewModel PaginationSettings { get; set; } = new();
    public int TotalCount { get; set; }
}

public class HomeTrackCardViewModel
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = "Learning Track";
    public string CategoryIcon { get; set; } = "fa-laptop-code";
    public string Level { get; set; } = "Beginner";
    public string LevelBadgeClass => Level.ToLower();
    
    // Statistics
    public int CoursesCount { get; set; }
    public int DurationHours { get; set; }
    public int StudentsCount { get; set; }
    public decimal Rating { get; set; } = 4.8m;
    
    // Skills and content
    public List<string> Skills { get; set; } = new();
    public List<HomeCoursePreviewViewModel> CoursesPreview { get; set; } = new();
}

public class HomeCoursePreviewViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Intermediate";
}

// DTO for filtering/sorting tracks
public class BrowseTracksRequestDto
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public string? SortBy { get; set; } = "popularity";
    public string? SortOrder { get; set; } = "desc";
}