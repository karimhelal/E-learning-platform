using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;

namespace Web.Controllers.Student;

[Authorize(Roles = "Student")]
public abstract class StudentBaseController(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    AppDbContext context,
    ILogger logger) : Controller
{
    protected async Task<int> GetStudentIdAsync()
    {
        var userId = currentUserService.GetUserId();
        if (userId == 0) return 0;

        var studentProfile = await userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);

        if (studentProfile == null)
        {
            try
            {
                studentProfile = new StudentProfile { UserId = userId, Bio = "New learner on the platform" };
                context.StudentProfiles.Add(studentProfile);
                await context.SaveChangesAsync();
                studentProfile = await userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating student profile");
                return 0;
            }
        }
        return studentProfile?.StudentId ?? 0;
    }
}