using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface IInstructorProfileRepository : IGenericRepository<InstructorProfile>
{
    IQueryable<CourseEnrollment>? GetAllInstructorEnrollmentsQueryable(int instructorId);
}
