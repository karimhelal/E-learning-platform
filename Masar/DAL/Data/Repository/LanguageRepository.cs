using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _context;
    
    public LanguageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Language entity)
    {
        await _context.Languages.AddAsync(entity);
    }

    public bool Delete(int id)
    {
        var language = _context.Languages.Find(id);
        if (language == null) return false;
        
        _context.Languages.Remove(language);
        return true;
    }

    public bool Delete(Language entity)
    {
        _context.Languages.Remove(entity);
        return true;
    }

    public async Task<IEnumerable<Language>> GetAllAsync()
    {
        return await _context.Languages.ToListAsync();
    }

    public IQueryable<Language> GetAllQueryable()
    {
        return _context.Languages
            .AsQueryable()
            .AsNoTracking();
    }

    public async Task<Language?> GetByIdAsync(int id)
    {
        return await _context.Languages
            .FirstOrDefaultAsync(l => l.LanguageId == id);
    }

    public bool Update(Language entity)
    {
        _context.Languages.Update(entity);
        return true;
    }
}
