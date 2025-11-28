using BLL.DTOs.Account;
using BLL.Interfaces.Account;
using Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
                return (result, user);
            }
            return (result,null);
        }


        
    }
}
