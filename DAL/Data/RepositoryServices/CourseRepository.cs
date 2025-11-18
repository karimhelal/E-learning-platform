using Microsoft.EntityFrameworkCore;
using Core.RepositoryInterfaces;
using Core.Entities;

namespace DAL.Data.RepositoryServices;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    public CourseRepository(AppDbContext context) => _context = context;


    public async Task<Course?> GetByIdAsync(int courseId)
    {
        return await _context.Courses.FindAsync(courseId);
    }

    public IQueryable<Course> GetAllQueryable()
    {
        return _context.Courses;
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        // save changes happen in the Service Layer (BLL)
    }

    public bool Update(Course course)
    {
        if (course == null || GetByIdAsync(course.Id).Result == null)
            return false;

        _context.Courses.Update(course);
        return true;
        // save changes happen in the Service Layer (BLL)
    }

    public bool Delete(int id)
    {
        Course? course = GetByIdAsync(id).Result;

        if (course == null)
            return false;

        _context.Courses.Remove(course);
        return true;
        // save changes happen in the Service Layer (BLL)
    }

    public bool Delete(Course course)
    {
        if (!_context.Courses.Contains(course))
            return false;

        _context.Courses.Remove(course);
        return true;
        // save changes happen in the Service Layer (BLL)
    }



    public IQueryable<Course> GetCoursesByInstructorQueryable(int instrutorId)
    {
        return _context.Courses
            .Where(c => c.InstructorId == instrutorId)
            .AsNoTracking();
    }
}
