namespace BLL.DTOs.Student;

public class StudentDashboardDto
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; }

    // Stats
    public StudentGeneralStatsDto GeneralStats { get; set; }
    public IEnumerable<StudentContinueLearningCourseCardDto> ContinueLearningCourses { get; set; }
    public IEnumerable<StudentEnrolledTrackCardDto>  EnrolledTracks { get; set; }
}


public class StudentGeneralStatsDto
{
    public int ActiveTracks { get; set; }
    public int EnrolledCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CertificatesEarned { get; set; }
}

public class StudentRecentActivityDto
{

}

public class StudentContinueLearningCourseCardDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }

    public string MainCategoryName { get; set; } = string.Empty;
    public string MainCategoryIcon { get; set; } = "fa-laptop-code";
    public string MainCategoryBadgeClass { get; set; } = "badge-purple";
    
    public string LevelName { get; set; } = string.Empty;
    public string LevelClass { get; set; } = "beginner";
    
    public string InstructorName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public int DurationHours { get; set; }

}

public class StudentEnrolledTrackCardDto
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
