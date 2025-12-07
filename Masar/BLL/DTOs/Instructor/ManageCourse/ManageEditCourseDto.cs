using Core.Entities.Enums;

namespace BLL.DTOs.Instructor.ManageCourse;

public class ManageEditCourseDto
{
}

public class ManageEditLessonDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public LessonContentType ContentType { get; set; }

    public string? VideoUrl { get; set; }
    public int? DurationInMinutes { get; set; }
    public string? ArticleContent { get; set; }

    public IEnumerable<ManageEditLessonResourceDto> Resources { get; set; }
}

public class ManageEditLessonResourceDto
{
    public int LessonResourceId { get; set; }
    public LessonResourceType ResourceType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}
