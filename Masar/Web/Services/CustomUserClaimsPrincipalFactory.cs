using Core.Entities;
using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Web.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole<int>>
    {
        private readonly AppDbContext _context;

        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            AppDbContext context)
            : base(userManager, roleManager, optionsAccessor)
        {
            _context = context;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            var studentId = await _context.StudentProfiles
                .Where(s => s.UserId == user.Id)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (studentId > 0)
            {
                identity.AddClaim(new Claim("StudentId", studentId.ToString()));
            }

            var instructorId = await _context.InstructorProfiles
                .Where(i => i.UserId == user.Id)
                .Select(i => i.InstructorId)
                .FirstOrDefaultAsync();

            if (instructorId > 0)
            {
                identity.AddClaim(new Claim("InstructorId", instructorId.ToString()));
            }

           
            return identity;
        }
    }
}
