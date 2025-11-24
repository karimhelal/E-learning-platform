using BLL.DTOs.Course.Module;
using Core.Entities;

namespace BLL.DTOs.Course;

public class InstructorCourseContentDetailsDto
{
    public int CourseId { get; set; }
    public IEnumerable<ModuleDetailsDto> ModulesDetails { get; set; }


    // reference to the owning entity
    public int InstructorId { get; set; }
}