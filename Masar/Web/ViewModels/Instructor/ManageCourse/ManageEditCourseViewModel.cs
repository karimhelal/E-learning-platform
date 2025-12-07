using Core.Entities.Enums;

namespace Web.ViewModels.Instructor.ManageCourse;

public class ManageEditCourseViewModel
{

}

public class ManageEditLessonViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public string ContentType { get; set; }

    public string? VideoUrl { get; set; }
    public int? DurationInMinutes { get; set; }
    public string? ArticleContent { get; set; }

    public IEnumerable<ManageEditLessonResourceViewModel> Resources { get; set; }
}

public class ManageEditLessonResourceViewModel
{
    public int LessonResourceId { get; set; }
    public string ResourceType { get; set; }
    public string Url { get; set; }
    public string? Title { get; set; }
}
