using BLL.DTOs.Instructor.ManageCourse;

namespace BLL.Interfaces.Instructor;

public interface IInstructorManageCourseService
{
    Task<ManageViewCourseDto?> GetCourseForManageAsync(int instructorId, int courseId);
    Task<ManageViewCourseCurriculumDto?> GetCourseCurriculumAsync(int instructorId, int courseId);
    Task<ManageEditLessonDto?> GetLessonDataAsync(int instructorId, int lessonId);

    // Add/Update/Delete Module
    Task<ManageSaveResultDto> SaveModuleAsync(int instructorId, int courseId, ManageSaveModuleDto moduleDto);
    Task<bool> DeleteModuleAsync(int instructorId, int courseId, int moduleId);

    // Add/Update/Delete Lesson
    Task<ManageSaveResultDto> SaveLessonAsync(int instructorId, int courseId, ManageSaveLessonDto lessonDto);
    Task<bool> DeleteLessonAsync(int instructorId, int courseId, int lessonId);

    // Basic Info
    Task<ManageSaveResultDto> SaveBasicInfoAsync(int instructorId, int courseId, ManageSaveBasicInfoDto basicInfoDto);

    // Learning Outcomes
    Task<ManageSaveResultDto> SaveLearningOutcomeAsync(int instructorId, int courseId, ManageSaveLearningOutcomeDto outcomeDto);
    Task<bool> DeleteLearningOutcomeAsync(int instructorId, int courseId, int outcomeId);
}

