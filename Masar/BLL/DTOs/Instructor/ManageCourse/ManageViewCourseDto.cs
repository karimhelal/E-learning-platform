using Core.Entities.Enums;

namespace BLL.DTOs.Instructor.ManageCourse;

public class ManageViewCourseDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public LearningEntityStatus CourseStatus { get; set; }

    public ManageViewCourseCurriculumDto? Curriculum { get; set; }
}


public class ManageViewCourseCurriculumDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public LearningEntityStatus CourseStatus { get; set; }

    public IEnumerable<ManageViewModuleDto> Modules { get; set; }
}


public class ManageViewModuleDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; }

    public int ModuleOrder { get; set; }
    public int LessonsCount { get; set; }
    public int? DurationInMinutes { get; set; }

    public IEnumerable<ManageViewLessonDto> Lessons { get; set; }
}

public class ManageViewLessonDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public LessonContentType ContentType { get; set; }
    public int? DurationInMinutes { get; set; }
}


