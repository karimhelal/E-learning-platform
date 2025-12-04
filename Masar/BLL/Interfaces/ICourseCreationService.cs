using BLL.DTOs.Course;

namespace BLL.Interfaces
{
    public interface ICourseCreationService
    {
        Task<CourseCreationResultDto> CreateCourseAsync(CreateCourseDto dto);
    }
}