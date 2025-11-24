using BLL.DTOs.Course;
using BLL.DTOs.Misc;


namespace BLL.Interfaces.Instructor;

public interface IInstructorCoursesService
{
    Task<PagedResult<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequest request);
}
