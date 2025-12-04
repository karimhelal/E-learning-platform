using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    
    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity);
    }

    public bool Delete(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null) return false;
        
        _context.Categories.Remove(category);
        return true;
    }

    public bool Delete(Category entity)
    {
        _context.Categories.Remove(entity);
        return true;
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

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public bool Update(Category entity)
    {
        _context.Categories.Update(entity);
        return true;
    }
}
