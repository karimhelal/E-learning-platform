using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class InstructorRepository : IInstructorRepository
{
    private readonly AppDbContext _context;
    public InstructorRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(InstructorProfile entity)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(InstructorProfile entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InstructorProfile>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public IQueryable<InstructorProfile> GetAllQueryable()
    {
        throw new NotImplementedException();
    }

    public Task<InstructorProfile?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public IQueryable<InstructorProfile?> GetProfile(int instructorId)
    {
        return _context.InstructorProfiles
            .Where(ip => ip.InstructorId == instructorId)
            .AsNoTracking();
    }

    public bool Update(InstructorProfile entity)
    {
        throw new NotImplementedException();
    }
}
