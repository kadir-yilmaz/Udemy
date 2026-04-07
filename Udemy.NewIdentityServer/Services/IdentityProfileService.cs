using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Udemy.NewIdentityServer.Models;

namespace Udemy.NewIdentityServer.Services
{
    public class IdentityProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                var sub = context.Subject.GetSubjectId();
                var user = await _userManager.FindByIdAsync(sub);

                if (user == null)
                {
                    return;
                }

                var name = user.UserName;
                try
                {
                    if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Surname))
                    {
                        name = $"{user.Name} {user.Surname}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IdentityProfileService] Error reading Name/Surname: {ex.Message}");
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim("name", name)
                };

                if (user.Id != null) claims.Add(new Claim("sub", user.Id));

                context.IssuedClaims = claims;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IdentityProfileService] Fatal Error in GetProfileDataAsync: {ex.Message}");
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(subjectId);
            context.IsActive = user != null;
        }
    }
}
