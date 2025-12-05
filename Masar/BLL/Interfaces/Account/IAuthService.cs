using BLL.DTOs.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;


namespace BLL.Interfaces.Account;

public interface IAuthService
{
    Task<(IdentityResult Result, User? user)> RegisterUserAsync(RegisterDto registerDto);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
}
