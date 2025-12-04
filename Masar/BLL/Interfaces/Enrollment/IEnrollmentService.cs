namespace BLL.Interfaces.Enrollment;

public interface IEnrollmentService
{
    Task<bool> IsStudentEnrolledAsync(int studentId, int coursesId);
    Task<int?> GetCourseIdIfEnrolled(int studentId, int lessonId);
}
