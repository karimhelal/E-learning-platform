using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class InstructorProfileRepository : IInstructorProfileRepository
{
    private readonly AppDbContext _context;
    public InstructorProfileRepository (AppDbContext context)
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

    public bool Update(InstructorProfile entity)
    {
        throw new NotImplementedException();
    }




    public IQueryable<CourseEnrollment>? GetAllInstructorEnrollmentsQueryable(int instructorId)
    {
        return _context.InstructorProfiles
            .Where(p => p.InstructorId == instructorId)
            .SelectMany(p => p.OwnedCourses)
            .SelectMany(c => c.Enrollments)
            .AsQueryable()
            .AsNoTracking();
    }


    public async Task<bool> HasCourseAsync(int instructorId, int courseId)
    {
        return (await _context.Courses.FindAsync(courseId))?.InstructorId == instructorId;
    }
}
