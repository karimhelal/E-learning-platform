using BLL.DTOs.Classroom;

namespace BLL.Interfaces.CourseLearning;

public interface ICourseLearningService
{
    Task<ClassroomDto?> GetClassroomAsync(int studentId, int courseId, int? lessonId = null);

    Task<LessonPlayerDto?> GetLessonDataAsync(int studentId, int lessonId);
    Task<CourseProgressDto> UpdateLessonCompletionState(int studentId, int lessonId, bool newCompletionState);
    Task<bool> MarkLessonStartedAsync(int studentId, int lessonId);
}
