using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    public LessonRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Lesson entity)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(Lesson entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Lesson>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public IQueryable<Lesson> GetAllQueryable()
    {
        throw new NotImplementedException();
    }

    public Task<Lesson?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Lesson> GetByIdQueryable(int lessonId)
    {
        return _context.Lessons
            .Where(l => l.LessonId == lessonId)
            .AsNoTracking();
    }

    public IQueryable<Course?> GetContainingCourseQueryable(int lessonId)
    {
        return _context.Lessons
            .Where(l => l.LessonId == lessonId)
            .Select(l => l.Module!.Course)
            .AsNoTracking();
    }

    public async Task<Course?> GetContainingCourseAsync(int lessonId)
    {
        return await _context.Lessons
            .Where(l => l.LessonId == lessonId)
            .Select(l => l.Module.Course)
            .SingleOrDefaultAsync();
    }

    public bool Update(Lesson entity)
    {
        throw new NotImplementedException();
    }
}
