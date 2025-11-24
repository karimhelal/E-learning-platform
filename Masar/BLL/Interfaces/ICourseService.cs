using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;


namespace BLL.Interfaces;

public interface ICourseService
{
    Task<PagedResult<InstructorCourseDto>> GetCoursesByInctructorAsync(int instructorId, PagingRequest request);
    Task<InstructorCourseEditResult> GetInstructorCourseForEditorAsync(int instructorId, int courseId, InstructorCourseEditRequest request);
    InstructorCourseBasicDetailsDto? GetInstructorCourseBasicDetails(int instructorId, int courseId);
    InstructorCourseContentDetailsDto? GetInstructorCourseContentDetails(int instructorId, int courseId);
    IQueryable<InstructorTopPerformingCourseDto> GetInstructorTopPerformingCourses(int instructorId, int topN);
}
