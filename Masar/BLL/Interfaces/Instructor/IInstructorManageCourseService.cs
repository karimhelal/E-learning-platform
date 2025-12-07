using BLL.DTOs.Instructor.ManageCourse;

namespace BLL.Interfaces.Instructor;

public interface IInstructorManageCourseService
{
    Task<ManageViewCourseDto?> GetCourseForManageAsync(int instructorId, int courseId);
    Task<ManageViewCourseCurriculumDto?> GetCourseCurriculumAsync(int instructorId, int courseId);

    Task<ManageEditLessonDto?> GetLessonDataAsync(int instructorId, int lessonId);
}

