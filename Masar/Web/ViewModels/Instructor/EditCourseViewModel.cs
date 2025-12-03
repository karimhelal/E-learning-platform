namespace Web.ViewModels.Instructor;

public class EditCourseViewModel
{
    public EditCourseDataViewModel Data { get; set; } = new();
    public string PageTitle { get; set; } = "Edit Course";
}

public class EditCourseDataViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    // Category & Level - Changed from int to string
    public string SelectedCategoryId { get; set; } = string.Empty; // ? Changed here
    public string SelectedLevel { get; set; } = string.Empty;
    public List<SelectOption> AvailableCategories { get; set; } = new();
    public List<SelectOption> AvailableLevels { get; set; } = new();
    
    // Learning Outcomes
    public List<string> LearningOutcomes { get; set; } = new();
    
    // Stats
    public EditCourseStatsViewModel Stats { get; set; } = new();
    
    // Curriculum
    public List<EditModuleViewModel> Modules { get; set; } = new();
    
    // Students
    public List<EnrolledStudentViewModel> EnrolledStudents { get; set; } = new();
    public int TotalStudentPages { get; set; }
}

public class EditCourseStatsViewModel
{
    public int EnrolledStudents { get; set; }
    public float AverageRating { get; set; }
    public int Completions { get; set; }
    public int AverageProgress { get; set; }
}

public class EditModuleViewModel
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public int LessonsCount { get; set; }
    public string DurationFormatted { get; set; } = string.Empty;
    public List<EditLessonViewModel> Lessons { get; set; } = new();
}

public class EditLessonViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public int ContentType { get; set; }
    public string? VideoUrl { get; set; }
    public string? PdfUrl { get; set; }
    public int DurationInSeconds { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
    public string TypeClass { get; set; } = string.Empty;
    public string TypeIcon { get; set; } = string.Empty;
    public string DurationFormatted { get; set; } = string.Empty;
    public List<EditLessonResourceViewModel> Resources { get; set; } = new();
}

public class EditLessonResourceViewModel
{
    public int LessonResourceId { get; set; }
    public int ResourceType { get; set; }
    public string ResourceTypeName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public class EnrolledStudentViewModel
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string EnrolledDate { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public string LastActivity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
}

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}