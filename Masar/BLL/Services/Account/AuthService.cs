using BLL.DTOs.Account;
using BLL.Interfaces.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BLL.Services.Account
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;

        public AuthService(UserManager<User> userManager)
        {
            _userManager = userManager;
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

            var result = await _userManager.CreateAsync(user,registerDto.Password);

            if(!result.Succeeded)
            {
                return (result, null);
            }


            var addRoleResult = await _userManager.AddToRoleAsync(user, "Student");

            if (!addRoleResult.Succeeded)
            {
                // Roll back user creation to avoid partial state (user without the expected role)
                await _userManager.DeleteAsync(user);
                return (addRoleResult, null);
            }

            return (result, user);
        }

    }
}
