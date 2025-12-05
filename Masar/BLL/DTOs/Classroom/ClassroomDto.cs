namespace BLL.DTOs.Classroom;

public class ClassroomDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    
    // Course Overview Stats
    public CourseOverviewStatsDto CourseOverviewStats { get; set; }

    // Sidebar
    public CourseProgressDto CourseProgress { get; set; }

    // Main Content
    public LessonPlayerDto CurrentLesson { get; set; }
    public IEnumerable<ModuleSidebarDto> Curriculum { get; set; }
}

public class CourseOverviewStatsDto
{
    public string? CourseDescription { get; set; }

    public int ModulesCount { get; set; }
    public int LessonsCount { get; set; }
    public int TotalDurationInMinutes { get; set; }
    public int TotalReviewsCount { get; set; }
    public float AverageRating { get; set; }

    public int EnrolledStudentsCount { get; set; }

    public string Level { get; set; }
    public IEnumerable<string> Categories { get; set; }
    public IEnumerable<string> Languages { get; set; }
    public IEnumerable<string> LearningOutcomes { get; set; }

    public CourseInstructorStatsDto InstructorStats { get; set; }
}


public class CourseInstructorStatsDto
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }
    public string? RoleTitle { get; set; }
    public string? Bio { get; set; }

    public int? YearsOfExperience { get; set; }
    public float AverageRating { get; set; }
    public int StudentsTaughtCount { get; set; }
    public int TotalCoursesCount { get; set; }

    public string? ProfilePictureUrl { get; set; }
}


public class CourseProgressDto
{
    public int TotalLessonsCount { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int ProgressPercentage { get; set; }
}


public class LessonPlayerDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }
    public string ModuleTitle { get; set; }

    public string ContentType { get; set; }
    public string? VideoUrl { get; set; }
    public string? ArticleContent { get; set; }

    public bool IsCompleted { get; set; }

    public IEnumerable<LessonResourceDto> Resources { get; set; }
}

public class ModuleSidebarDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; }

    public int ModuleOrder { get; set; }
    public int LessonsCount { get; set; }
    public int TotalDurationInMinutes { get; set; }

    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<LessonSidebarDto> Lessons { get; set; }

}

public class LessonSidebarDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public int TotalDurationInMinutes { get; set; }
    public string ContentType { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsActive { get; set; }
}



public class LessonResourceDto
{
    public int LessonResourceId { get; set; }

    public string ResourceTitle { get; set; }
    public string ResourceType { get; set; }
    public string ResourceUrl { get; set; }
}