using BLL.DTOs.Account;
using BLL.Interfaces.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using DAL.Data;

namespace BLL.Services.Account
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AuthService(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;


            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<(IdentityResult Result,User user)> RegisterUserAsync(RegisterDto registerDto)
        {
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if(!result.Succeeded)
            {
                return (result, null);
            }

            // Add BOTH Student and Instructor roles
            var studentRoleResult = await _userManager.AddToRoleAsync(user, "Student");
            if (!studentRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return (studentRoleResult, null);
            }

            var instructorRoleResult = await _userManager.AddToRoleAsync(user, "Instructor");
            if (!instructorRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return (instructorRoleResult, null);
            }

            // Create StudentProfile
            var studentProfile = new StudentProfile
            {
                UserId = user.Id,
                Bio = "New learner on the platform"
            };
            _context.StudentProfiles.Add(studentProfile);

            // Create InstructorProfile
            var instructorProfile = new InstructorProfile
            {
                UserId = user.Id,
                Bio = "New instructor on the platform",
                YearsOfExperience = 0
            };
            _context.InstructorProfiles.Add(instructorProfile);

            await _context.SaveChangesAsync();

            return (result, user);
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
