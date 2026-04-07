using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Udemy.IdentityServer.Models;

namespace Udemy.IdentityServer.Services
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
                    // Fallback for non-existent user (shouldn't happen with valid token)
                    return;
                }

                // Defensive check: If DB columns are missing, accessing Name/Surname might fail in some setups,
                // or if values are null.
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
                // context.IssuedClaims = new List<Claim>(); // Return empty claims rather than crashing
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
