using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data.RepositoryServices;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }


    public Task AddAsync(User user)
    {
        throw new NotImplementedException();
    }

    public bool Update(User user)
    {
        throw new NotImplementedException();
    }

    public bool Delete(int userId)
    {
        throw new NotImplementedException();
    }

    public bool Delete(User user)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public IQueryable<User> GetAllQueryable()
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public Task<InstructorProfile?> GetInstructorProfileForUserAsync(int userId, bool includeUserBase)
    {
        var query = _context.InstructorProfiles.AsQueryable();

        if (includeUserBase)
            query = query.Include(ip => ip.User);

        return query.FirstOrDefaultAsync(ip => ip.UserId == userId);
    }

    public Task<StudentProfile?> GetStudentProfileForUserAsync(int userId, bool includeUserBase)
    {
        var query = _context.StudentProfiles.AsQueryable();

        if (includeUserBase)
            query = query.Include(sp => sp.User);

        return query.FirstOrDefaultAsync(sp => sp.UserId == userId);
    }

    public bool IsAdmin(int userId)
    {
        return _context.UserRoles.Any(UserRoles => UserRoles.UserId == userId && UserRoles.RoleId == (int)UserRolesEnum.Admin);
        //return _context.Users.Any(u => u.Id == userId);
    }

    public bool IsInstructor(int userId)
    {
        return _context.UserRoles.Any(UserRoles => UserRoles.UserId == userId && UserRoles.RoleId == (int)UserRolesEnum.Instructor);
        //return _context.Users.Any(u => u.Id == userId);
    }

    public bool IsStudent(int userId)
    {
        return _context.UserRoles.Any(UserRoles => UserRoles.UserId == userId && UserRoles.RoleId == (int)UserRolesEnum.Student);
        //return _context.Users.Any(u => u.Id == userId);
    }

    public Task<InstructorProfile?> GetInstructorProfileAsync(int instructorId, bool includeUserBase)
    {
        var query = _context.InstructorProfiles.AsQueryable();

        if (includeUserBase)
            query = query.Include(ip => ip.User);

        return query.FirstOrDefaultAsync(ip => ip.InstructorId == instructorId);
    }

    public Task<StudentProfile?> GetStudentProfileAsync(int studentId, bool includeUserBase)
    {
        var query = _context.StudentProfiles.AsQueryable();

        if (includeUserBase)
            query = query.Include(sp => sp.User);

        return query.FirstOrDefaultAsync(sp => sp.StudentId == studentId);

    }
}
