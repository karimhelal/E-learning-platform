using BLL.DTOs.Account;
using BLL.Interfaces.Account;
using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Account
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AuthService(
            UserManager<User> userManager, 
            AppDbContext context, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository
        )
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) 
                return null;

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }
        
        public async Task<(IdentityResult Result, User? user)> RegisterUserAsync(RegisterDto registerDto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // starting a transaction
                // ensures ATOMICITY: either ALL of this happens, or NONE of it happens.
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var user = new User
                    {
                        UserName = registerDto.Email,
                        Email = registerDto.Email,
                        FirstName = registerDto.FirstName,
                        LastName = registerDto.LastName,
                        Picture = "https://ui-avatars.com/api/?name=" + registerDto.FirstName + "+" + registerDto.LastName
                    };

                    // creating the user (Identity)
                    var result = await _userManager.CreateAsync(user, registerDto.Password);
                    if (!result.Succeeded)
                    {
                        // actually no need to delete cause nothing happened yet (the transaction haven't been started yet)
                        return (result, null);
                    }

                    // adding roles (all at once -> one db trip instead of two)
                    var roleResult = await _userManager.AddToRolesAsync(user, new[] { "Student", "Instructor" });
                    if (!roleResult.Succeeded)
                    {
                        // throwing an exception to indicate an error happened while trying to add roles
                        throw new Exception(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }

                    // creating the two profiles
                    // We add them directly using _context
                    // since 'user' is tracked by EF Core (when added by the identity) we can just link them
                    var studentProfile = new StudentProfile
                    {
                        UserId = user.Id,
                        Bio = "New learner on the platform"
                    };

                    var instructorProfile = new InstructorProfile
                    {
                        UserId = user.Id,
                        Bio = "New instructor on the platform",
                        YearsOfExperience = null
                    };

                    // adding profiles using _context (cached in memory for the transaction)
                    await _context.StudentProfiles.AddAsync(studentProfile);
                    await _context.InstructorProfiles.AddAsync(instructorProfile);

                    // save changes !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ---> the transaction could fail though so don't get excited just yet
                    await _context.SaveChangesAsync();

                    // committing the transaction
                    // only NOW does the data become permanent in the database =========================> YOU CAN CHEER UP FRIEND, OPERATION SUCCESSEDED
                    await transaction.CommitAsync();

                    return (IdentityResult.Success, user);
                }
                catch (Exception ex)
                {
                    // something went wrong? =====> EASY PEASY, just undo EVERYTHING and you're pretty much safe
                    await transaction.RollbackAsync();

                    // return a generic failure or log the specific exception 'ex'
                    return (IdentityResult.Failed(new IdentityError { Description = "Registration failed :(: " + ex.Message }), null);
                }
            });
        }


        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });


            return await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        }
    }
}
