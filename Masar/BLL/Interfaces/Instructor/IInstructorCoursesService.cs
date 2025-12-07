using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Instructor.CreateCourse;
using BLL.DTOs.Instructor.ManageCourse;
using BLL.DTOs.Misc;

namespace BLL.Interfaces.Instructor;

public interface IInstructorCoursesService
{
    Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequestDto request);
    Task<InstructorCourseEditDto?> GetCourseForEditAsync(int instructorId, int courseId);
    Task<bool> UpdateCourseAsync(int instructorId, int courseId, UpdateCourseDto updateDto);
    Task<bool> UpdateLessonAsync(int instructorId, int courseId, ManageEditLessonDto lessonEditDto);

    
    // Curriculum management methods
    Task<int?> CreateCourseAsync(int instructorId, CreateCourseBasicDetailsDto courseBasicDetailsDto);
    Task<UpdateResultDto> UpdateModuleAsync(int instructorId, int courseId, UpdateModuleDto moduleDto);
    Task<bool> DeleteModuleAsync(int instructorId, int courseId, int moduleId);
    Task<UpdateResultDto> UpdateLessonAsync(int instructorId, int courseId, UpdateLessonDto lessonDto);
    Task<bool> DeleteLessonAsync(int instructorId, int courseId, int lessonId);
}
