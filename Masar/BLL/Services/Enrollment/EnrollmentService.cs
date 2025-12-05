using BLL.Interfaces.Enrollment;
using Core.RepositoryInterfaces;

namespace BLL.Services.Enrollment;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepo;
    public EnrollmentService(IEnrollmentRepository enrollmentRepo)
    {
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<bool> IsStudentEnrolledAsync(int studentId, int coursesId)
    {
        return await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId, coursesId);
    }

    public async Task<int?> GetCourseIdIfEnrolled(int studentId, int lessonId)
    {
        return await _enrollmentRepo.GetCourseIdIfEnrolledAsync(studentId, lessonId);
    }
}
