using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface IUserRepository : IGenericRepository<User>
{
    bool IsInstructor(int userId);
    bool IsStudent(int userId);
    bool IsAdmin(int userId);

    bool HasStudentProfileWithId(int studentId);
    Task<bool> HasInstructorProfileWithIdAsync(int instructorId);

    Task<InstructorProfile?> GetInstructorProfileForUserAsync(int userId, bool includeUserBase);
    Task<InstructorProfile?> GetInstructorProfileAsync(int instructorId, bool includeUserBase);
    Task<StudentProfile?> GetStudentProfileForUserAsync(int userId, bool includeUserBase);
    Task<StudentProfile?> GetStudentProfileAsync(int studentId, bool includeUserBase);


    IQueryable<StudentProfile>? GetStudentProfileQueryable(int studentId);
    IQueryable<InstructorProfile>? GetInstructorProfileQueryable(int instructorId);
}
