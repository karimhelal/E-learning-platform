using BLL.DTOs.Misc;

namespace BLL.DTOs.Student;

// --- Browse Tracks DTOs ---
public class StudentBrowseTracksPageDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public BrowseTrackPageStatsDto Stats { get; set; } = new();
    public List<BrowseTrackItemDto> Tracks { get; set; } = new();
}
public class BrowseTrackPageStatsDto
{
    public int TotalTracks { get; set; }
    public int BeginnerTracks { get; set; }
    public int IntermediateTracks { get; set; }
    public int AdvancedTracks { get; set; }
}
public class BrowseTrackItemDto
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string LevelBadgeClass { get; set; } = string.Empty;
    public int CoursesCount { get; set; }
    public int DurationHours { get; set; }
    public int StudentsCount { get; set; }
    public decimal Rating { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<CoursePreviewDto> CoursesPreview { get; set; } = new();
    public string ActionText { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
}
public class CoursePreviewDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

// --- Certificates DTOs ---
public class StudentCertificatesDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public int TotalCertificates { get; set; }
    public int CourseCertificatesCount { get; set; }
    public int TrackCertificatesCount { get; set; }
    public List<CertificateItemDto> Certificates { get; set; } = new();
}
public class CertificateItemDto
{
    public int CertificateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IssuedDate { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? DownloadLink { get; set; }
    public string VerificationId { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
}

// --- My Courses DTOs ---
public class StudentCoursesDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public List<MyCourseItemDto> AllCourses { get; set; } = new();
    public List<MyCourseItemDto> InProgressCourses { get; set; } = new();
    public List<MyCourseItemDto> CompletedCourses { get; set; } = new();
    public int AllCoursesCount => AllCourses.Count;
    public int InProgressCount => InProgressCourses.Count;
    public int CompletedCount => CompletedCourses.Count;
}
public class MyCourseItemDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
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

// --- Dashboard DTOs ---
public class StudentDashboardDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public DashboardStatsDto Stats { get; set; } = new();
    public List<ContinueLearningCourseDto> ContinueLearningCourses { get; set; } = new();
    public List<EnrolledTrackDto> EnrolledTracks { get; set; } = new();
}
public class DashboardStatsDto
{
    public int ActiveTracks { get; set; }
    public int EnrolledCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CertificatesEarned { get; set; }
}
public class ContinueLearningCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "fa-laptop-code";
    public string CategoryBadgeClass { get; set; } = "badge-purple";
    public string InstructorName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public int DurationHours { get; set; }
}
public class EnrolledTrackDto
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

// --- Track Details & My Tracks DTOs ---
public class StudentTrackDetailsDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public decimal TotalProgress { get; set; }
    public List<TrackCourseItemDto> Courses { get; set; } = new();
}
public class TrackCourseItemDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
    public int DurationHours { get; set; }
    public decimal ProgressPercentage { get; set; }
}
public class StudentTracksDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";
    public TrackPageStatsDto Stats { get; set; } = new();
    public List<StudentTrackItemDto> Tracks { get; set; } = new();
}
public class TrackPageStatsDto
{
    public int TotalTracks { get; set; }
    public int InProgressTracks { get; set; }
    public int CompletedTracks { get; set; }
}
public class StudentTrackItemDto
{
    public int TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CoursesCount { get; set; }
    public int DurationHours { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string Status { get; set; } = "in-progress";
    public string IconClass { get; set; } = "fa-laptop-code";
    public string ActionText { get; set; } = "Continue Learning";
    public string ActionUrl { get; set; } = "#";
    public List<TrackCoursePreviewDto> Courses { get; set; } = new();
}
public class TrackCoursePreviewDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fa-book";
    public string Status { get; set; } = "completed";
}


public class StudentBrowseCoursesPageDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = "JD";

    public StudentBrowseSettingsDto Settings { get; set; } = new();

    public List<StudentCourseBrowseCardDto> Items { get; set; } = new();
}





public class StudentCourseBrowseCardDto
{
    public int CourseId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailImageUrl { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string MainCategory { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public string Level { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int NumberOfReviews { get; set; }
    public int NumberOfStudents { get; set; }
    public int NumberOfLectures { get; set; }
    public int NumberOfMinutes { get; set; }
    public bool IsEnrolled { get; set; }
    public decimal ProgressPercentage { get; set; }
}


public abstract class FilterGroupDto
{
    public string Title { get; set; } = string.Empty;
    public string RequestKey { get; set; } = string.Empty;
}

public class CheckboxFilterDto : FilterGroupDto
{
    public List<FilterOptionDto> FilterOptions { get; set; } = new();
}

public class FilterOptionDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsChecked { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class NumberRangeFilterDto : FilterGroupDto
{
    public string MinRequestKey { get; set; } = string.Empty;
    public string MaxRequestKey { get; set; } = string.Empty;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double Step { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class DateRangeFilterDto : FilterGroupDto
{
    public string MinRequestKey { get; set; } = string.Empty;
    public string MaxRequestKey { get; set; } = string.Empty;
    public DateOnly MinDate { get; set; }
    public DateOnly MaxDate { get; set; }
}

public class StudentBrowseSettingsDto
{
    public List<FilterGroupDto> FilterGroups { get; set; } = new();
    public BLL.DTOs.Misc.PaginationSettingsDto PaginationSettings { get; set; } = new();
}