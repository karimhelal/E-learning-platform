using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _context;
    public LanguageRepository(AppDbContext context) {
        _context = context;
    }


    public Task AddAsync(Language entity)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(Language entity)
    {
        throw new NotImplementedException();
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

    public Task<Language?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public bool Update(Language entity)
    {
        throw new NotImplementedException();
    }
}
