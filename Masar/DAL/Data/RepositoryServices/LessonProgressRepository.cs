using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly AppDbContext _context;
    public LessonProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LessonProgress lessonProgress)
    {
        await _context.LessonProgress.AddAsync(lessonProgress);
    }

    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(LessonProgress entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LessonProgress>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public IQueryable<LessonProgress> GetAllLessonProgressForStudentQueryable(int studentId)
    {
        return _context.LessonProgress
            .Where(p => p.StudentId == studentId)
            .AsNoTracking();
    }

    public IQueryable<LessonProgress> GetAllQueryable()
    {
        throw new NotImplementedException();
    }

    public Task<LessonProgress?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public IQueryable<LessonProgress> GetStudentLessonProgressQueryable(int studentId, int lessonId)
    {
        return _context.LessonProgress
            .Where(l => l.StudentId == studentId && l.LessonId == lessonId)
            .AsNoTracking();
    }

    public async Task<LessonProgress?> GetStudentLessonProgressAsync(int studentId, int lessonId)
    {
        return await _context.LessonProgress
            .Where(l => l.StudentId == studentId && l.LessonId == lessonId)
            .FirstOrDefaultAsync();
    }


    public async Task AddOrUpdateAsync(LessonProgress lessonProgress)
    {
        if (!await _context.LessonProgress.AnyAsync(lp => lp.LessonProgressId == lessonProgress.LessonProgressId))
            await _context.LessonProgress.AddAsync(lessonProgress);
        else
            _context.LessonProgress.Update(lessonProgress);
    }


    public bool Update(LessonProgress entity)
    {
        throw new NotImplementedException();
    }
}
