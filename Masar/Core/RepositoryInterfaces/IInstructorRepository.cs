using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface IInstructorRepository : IGenericRepository<InstructorProfile>
{
    IQueryable<InstructorProfile?> GetProfile(int instructorId);
}
