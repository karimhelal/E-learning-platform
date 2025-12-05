using Core.Entities;

namespace Core.RepositoryInterfaces;

public interface ILessonRepository : IGenericRepository<Lesson>
{
    IQueryable<Lesson> GetByIdQueryable(int lessonId);
    IQueryable<Course?> GetContainingCourseQueryable(int lessonId);
    Task<Course?> GetContainingCourseAsync(int lessonId);
}
