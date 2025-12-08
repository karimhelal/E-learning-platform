namespace Web.ViewModels.Instructor.ManageCourse;

public class ManageSaveModuleViewModel
{
    public int ModuleId { get; set; }
    public int CourseId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
}

public class ManageSaveLessonViewModel
{
    public int LessonId { get; set; }
    public int ModuleId { get; set; }
    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Video";

    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public string? ArticleContent { get; set; }

    public List<ManageSaveLessonResourceViewModel>? Resources { get; set; }
}

public class ManageSaveLessonResourceViewModel
{
    public int LessonResourceId { get; set; }
    public string ResourceType { get; set; } = "URL";
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}