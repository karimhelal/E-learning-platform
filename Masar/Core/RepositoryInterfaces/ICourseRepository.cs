using Core.Entities;


namespace Core.RepositoryInterfaces;

public interface ICourseRepository : IGenericRepository<Course>
{
    IQueryable<Course> GetCoursesByInstructorQueryable(int instrutorId);
    IQueryable<Course> GetCourseByIdQueryable(int courseId);
}
