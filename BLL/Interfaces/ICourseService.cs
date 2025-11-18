using BLL.DTOs.Course;
using BLL.DTOs.Misc;


namespace BLL.Interfaces;

public interface ICourseService
{
    Task<PagedResult<InstructorCourseDto>> GetCoursesByInctructorAsync(int instructorId, PagingRequest request);
}
