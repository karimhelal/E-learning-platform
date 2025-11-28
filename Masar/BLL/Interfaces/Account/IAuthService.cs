using BLL.DTOs.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces.Account
{
    public interface IAuthService
    {
        Task<(IdentityResult Result,User user)> RegisterUserAsync(RegisterDto registerDto);
    }
}
