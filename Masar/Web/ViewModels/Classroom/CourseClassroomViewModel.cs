namespace Web.ViewModels.Classroom;

public class CourseClassroomViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }

    public ClassroomSidebarViewModel Sidebar { get; set; }
    public ClassroomMainContentViewModel MainContent { get; set; }
}

public class ClassroomSidebarViewModel
{
    public ClassroomSidebarStatsViewModel SidebarStats { get; set; }
    public IEnumerable<ModuleSidebarViewModel> SidebarModules { get; set; }
}

public class ClassroomSidebarStatsViewModel
{
    public CourseProgressViewModel CourseProgress { get; set; }

    public string FormattedCourseDuration { get; set; }
    public string FormattedRatingAndReviews { get; set; }
    public string FormattedNumberOfStudents { get; set; }

    public int TotalLessonsCount { get; set; }
}

public class CourseProgressViewModel
{
    public int ProgressPercentage { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int TotalLessonsCount { get; set; }
}

public class ModuleSidebarViewModel
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; }

    public int ModuleOrder { get; set; }
    public int LessonsCount { get; set; }
    public string FormattedModuleDuration { get; set; }

    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }

    public string CompletedCssClass => IsCompleted switch
    {
        true => "completed",
        false => (IsActive) ? "active" : ""
    };

    public string ActiveCssClass => IsActive switch
    {
        true => "open",
        false => ""
    };

    public IEnumerable<LessonSidebarViewModel> Lessons { get; set; }
}

public class LessonSidebarViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public string FormattedLessonDuration { get; set; }
    public string ContentType { get; set; }
    public string IconColorCssClass => ContentType switch
    {
        "Video" => "video",
        "Article" => "article",
        _ => "quiz"
    };

    public string IconTypeCssClass => ContentType switch
    {
        "Video" => "fas fa-play-circle",
        "Article" => "fas fa-file-alt",
        _ => "fas fa-clipboard-question"
    };

    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }

    public string CompletedCssClass => IsCompleted switch
    {
        true => "done",
        false => ""
    };

    public string ActiveCssClass => IsActive switch
    {
        true => "active",
        false => ""
    };

}



public class ClassroomMainContentViewModel
{
    public ClassroomCourseOverviewStatsViewModel CourseOverviewStats { get; set; }
    public LessonPlayerViewModel CurrentLesson { get; set; }
}

public class ClassroomCourseOverviewStatsViewModel
{
    public string CourseTitle { get; set; }
    public string CourseDescription { get; set; }

    public int ModuleCount { get; set; }
    public int LessonsCount { get; set; }

    public string FormattedDuration { get; set; }
    public string Level { get; set; }
    public string FormattedRatingAndReviews { get; set; }
    public string FormattedStudentsEnrolledCount { get; set; }

    public IEnumerable<string> Categories { get; set; }
    public IEnumerable<string> Languages { get; set; }
    public IEnumerable<string> LearningOutcomes { get; set; }

    public ClassroomCourseInstructorStatsViewModel InstructorStats { get; set; }
}

public class ClassroomCourseInstructorStatsViewModel
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }
    public string RoleTitle { get; set; }
    public string Bio { get; set; }

    public string YearsOfExperience { get; set; }
    public string FormattedRating { get; set; }
    public string FormattedStudentsTaughtCount { get; set; }
    public string FormattedCoursesCount { get; set; }

    public string ProfilePictureUrl { get; set; }
}

public class LessonPlayerViewModel
{
    public int LessonId { get; set; }
    public string ModuleTitle { get; set; }

    public LessonMetaDataViewModel Metadata { get; set; }
    public IEnumerable<LessonResourceViewModel> Resources { get; set; }
}

public class LessonMetaDataViewModel
{
    public string LessonTitle { get; set; }
    public string ContentType { get; set; }

    // For Video
    public string? VideoUrl { get; set; }

    // Helper to get YouTube ID (essential for embedding)
    public string YouTubeId
    {
        get
        {
            if (string.IsNullOrEmpty(VideoUrl)) return "";
            try
            {
                var uri = new Uri(VideoUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"] ?? "";
            }
            catch { return ""; }
        }
    }

    // For Article
    public string? ArticleContent { get; set; } // The HTML string from DB
}

public class LessonResourceViewModel
{
    public int LessonResourceId { get; set; }

    public string ResourceTitle { get; set; }
    public string ResourceType { get; set; }
    public string ResourceUrl { get; set; }

    public string IconColorCssClass => ResourceType switch
    {
        "PDF" => "pdf",
        "ZIP" => "zip",
        "URL" => "link",
        _ => "link"
    };

    public string IconTypeCssClass => ResourceType switch
    {
        "PDF" => "fas fa-file-pdf",
        "ZIP" => "fas fa-file-archive",
        "URL" => "fas fa-link",
        _ => "fas fa-link"
    };

    public string ActionIconTypeCssClass => ResourceType switch
    {
        "PDF" => "fas fa-download",
        "ZIP" => "fas fa-download",
        _ => "fas fa-external-link-alt"
    };
}