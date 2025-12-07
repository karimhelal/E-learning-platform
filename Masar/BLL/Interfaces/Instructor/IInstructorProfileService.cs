using BLL.DTOs.Instructor;

namespace BLL.Interfaces.Instructor;

public interface IInstructorProfileService
{
    Task<InstructorProfileDto?> GetInstructorProfileAsync(int instructorId);
    Task<bool> UpdateInstructorProfileAsync(int instructorId, InstructorProfileDto profileDto);

    Task<bool> HasCourseWithIdAsync(int instructorId, int courseId);
    Task<int?> GetCourseIdForLessonAsync(int instructorId, int lessonId);
    Task<bool> UpdateInstructorProfileAsync(int instructorId, UpdateInstructorProfileDto profileDto);
}