using BLL.DTOs.Student;

namespace BLL.Interfaces.Student;


public interface IStudentCourseDetailsService
{
    Task<StudentCourseDetailsDto?> GetCourseDetailsAsync(int studentId, int courseId);
    Task<bool> ToggleLessonCompletionAsync(int studentId, int lessonId, bool isCompleted);
}