using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.RepositoryInterfaces;

public interface IGenericRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    IQueryable<T> GetAllQueryable();
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T course);
    bool Delete(int id);
    bool Delete(T entity);
    bool Update(T entity);
}
