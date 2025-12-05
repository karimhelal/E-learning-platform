using Core.Entities.Enums;

namespace BLL.DTOs.Course;

public class InstructorCourseContentDetailsDto
{
    public int CourseId { get; set; }
    public IEnumerable<ModuleDetailsDto>? ModulesDetails { get; set; }


    // reference to the owning entity
    public int InstructorId { get; set; }
}

public class ModuleDetailsDto
{
    public int ModuleId { get; set; }
    public int ModuleOrder { get; set; }
    public string ModuleTitle { get; set; }
    public IEnumerable<LessonDetailsDto>? LessonsDetails { get; set; }


    // calculated fields
    public int TotalDurationInSeconds { get; set; }


    // Calculated properties (Computed on demand, extremely fast)
    public int Hours => TotalDurationInSeconds / 3600;
    public int Minutes => (TotalDurationInSeconds % 3600) / 60;


    // reference to the owning entity
    public int CourseId { get; set; }
}


public class LessonDetailsDto
{
    public int LessonId { get; set; }
    public int LessonOrder { get; set; }
    public string LessonTitle { get; set; }
    public int DurationInSeconds { get; set; }
    public LessonContentType LessonContentType { get; set; }



    // reference to the owning entity
    public int ModuleId { get; set; }
}
