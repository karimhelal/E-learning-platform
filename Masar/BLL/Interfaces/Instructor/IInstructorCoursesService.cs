using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;


namespace BLL.Interfaces.Instructor;

public interface IInstructorCoursesService
{
    Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequestDto request);
    Task<BLL.DTOs.Instructor.InstructorCourseEditDto?> GetCourseForEditAsync(int instructorId, int courseId);
    Task<bool> UpdateCourseAsync(int instructorId, int courseId, UpdateCourseDto updateDto);
    
    // Curriculum management methods
    Task<bool> UpdateModuleAsync(int instructorId, int courseId, UpdateModuleDto moduleDto);
    Task<bool> DeleteModuleAsync(int instructorId, int courseId, int moduleId);
    Task<bool> UpdateLessonAsync(int instructorId, int courseId, UpdateLessonDto lessonDto);
    Task<bool> DeleteLessonAsync(int instructorId, int courseId, int lessonId);
}
