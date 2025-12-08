using Core.Entities.Enums;

namespace BLL.DTOs.Instructor.ManageCourse;

public class ManageSaveResultDto
{
    public bool Success { get; set; }
    public int? EntityId { get; set; }
    public string? Message { get; set; }
}

public class ManageSaveModuleDto
{
    public int ModuleId { get; set; } // 0 for new module
    public int CourseId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
}

public class ManageSaveLessonDto
{
    public int LessonId { get; set; } // 0 for new lesson
    public int ModuleId { get; set; }
    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string ContentType { get; set; } = "Video"; // "Video" or "Article"

    public string? VideoUrl { get; set; }
    public int? DurationMinutes { get; set; }
    public string? ArticleContent { get; set; }

    public List<ManageSaveLessonResourceDto>? Resources { get; set; }
}

public class ManageSaveLessonResourceDto
{
    public int LessonResourceId { get; set; } // 0 for new resource
    public string ResourceType { get; set; } = "URL"; // "PDF", "ZIP", "URL"
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ManageSaveBasicInfoDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public string? ThumbnailImageUrl { get; set; }
    public string Level { get; set; } = "Beginner";
    public List<int> CategoryIds { get; set; } = [];
    public List<int> LanguageIds { get; set; } = [];
}

public class ManageSaveLearningOutcomeDto
{
    public int LearningOutcomeId { get; set; } // 0 for new outcome
    public int CourseId { get; set; }
    public string OutcomeName { get; set; } = string.Empty;
}