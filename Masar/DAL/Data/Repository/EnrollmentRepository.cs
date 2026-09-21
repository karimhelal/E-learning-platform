using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;
    public EnrollmentRepository(AppDbContext context)
    {
        _context = context;
    }


    public Task AddAsync(EnrollmentBase entity)
    {
        throw new NotImplementedException();
    }

    public Task AddEnrollmentAsync(EnrollmentBase enrollment)
    {
        throw new NotImplementedException();
    }


    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(EnrollmentBase entity)
    {
        throw new NotImplementedException();
    }

    public void DeleteEnrollment(EnrollmentBase enrollment)
    {
        throw new NotImplementedException();
    }


    public Task<IEnumerable<EnrollmentBase>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public IQueryable<EnrollmentBase> GetAllQueryable()
    {
        throw new NotImplementedException();
    }

    public Task<EnrollmentBase?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }


    public Task<CourseEnrollment?> GetCourseEnrollmentAsync(int studentId, int courseId)
    {
        throw new NotImplementedException();
    }

    public IQueryable<CourseEnrollment> GetStudentCourseEnrollmentQueryable(int studentId, int courseId)
    {
        return _context.CourseEnrollments
            .Where(e => e.StudentId == studentId && e.CourseId == courseId)
            .AsNoTracking();
    }

    public IQueryable<EnrollmentBase> GetStudentEnrollmentsQueryable(int studentId)
    {
        throw new NotImplementedException();
    }


    public async Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId)
    {
        return await _context.CourseEnrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }


    public async Task<int?> GetCourseIdIfEnrolledAsync(int studentId, int lessonId)
    {
        return await _context.Lessons
            .AsSplitQuery()
            .Where(l => l.LessonId == lessonId)
            .Select(l => l.Module.Course)
            .Where(c => c.Enrollments.Any(e => e.StudentId == studentId & e.CourseId == c.Id))
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }



    public async Task<bool> IsStudentEnrolledInTrackAsync(int studentId, int trackId)
    {
        return await _context.TrackEnrollments
            .AnyAsync(e => e.StudentId == studentId && e.TrackId == trackId);
    }


    public bool Update(EnrollmentBase entity)
    {
        throw new NotImplementedException();
    }

    public void UpdateEnrollment(EnrollmentBase enrollment)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateProgressPercentageAsync(int studentId, int courseId, decimal progressPercentage)
    {
        var courseEnrollment = await _context.CourseEnrollments
            .SingleOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

        if (courseEnrollment == null)
            throw new InvalidOperationException("enrollment doesn't exist, either the course is missing or the student is missing or the student is not enrolled in the course");

        courseEnrollment.ProgressPercentage = progressPercentage;
    }
}
