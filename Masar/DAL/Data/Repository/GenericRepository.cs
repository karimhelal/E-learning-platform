using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public IQueryable<T> GetAllQueryable()
    {
        return _dbSet.AsQueryable().AsNoTracking();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        // Note: SaveChanges should be called in the service layer
    }

    public bool Update(T entity)
    {
        _dbSet.Update(entity);
        return true;
        // Note: SaveChanges should be called in the service layer
    }

    public bool Delete(int id)
    {
        var entity = _dbSet.Find(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        return true;
        // Note: SaveChanges should be called in the service layer
    }

    public bool Delete(T entity)
    {
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        return true;
        // Note: SaveChanges should be called in the service layer
    }
}