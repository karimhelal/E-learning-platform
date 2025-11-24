using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface IUserRepository : IGenericRepository<User>
{
    bool IsInstructor(int userId);
    bool IsStudent(int userId);
    bool IsAdmin(int userId);

    Task<InstructorProfile?> GetInstructorProfileForUserAsync(int userId, bool includeUserBase);
    Task<InstructorProfile?> GetInstructorProfileAsync(int instructorId, bool includeUserBase);
    Task<StudentProfile?> GetStudentProfileForUserAsync(int userId, bool includeUserBase);
    Task<StudentProfile?> GetStudentProfileAsync(int studentId, bool includeUserBase);
}
