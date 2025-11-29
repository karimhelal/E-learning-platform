using BLL.DTOs.Course;
using BLL.DTOs.Misc;


namespace BLL.Interfaces.Instructor;

public interface IInstructorCoursesService
{
    Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequestDto request);
}
