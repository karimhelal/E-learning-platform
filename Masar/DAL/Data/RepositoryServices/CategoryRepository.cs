using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository (AppDbContext context) {
        _context = context;
    }


    public Task AddAsync(Category entity)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(Category entity)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
       return await _context.Categories.ToListAsync();
    }

    public IQueryable<Category> GetAllQueryable()
    {
        return _context.Categories
            .AsQueryable()
            .AsNoTracking();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public bool Update(Category entity)
    {
        throw new NotImplementedException();
    }
}
