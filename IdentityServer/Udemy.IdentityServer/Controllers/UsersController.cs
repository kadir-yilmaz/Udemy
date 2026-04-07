using IdentityServer4.Hosting.LocalApiAuthentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Udemy.IdentityServer.Dtos;
using Udemy.IdentityServer.Models;
using static IdentityServer4.IdentityServerConstants;

namespace Udemy.IdentityServer.Controllers
{
    [Authorize(LocalApi.PolicyName)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignupDto signupDto)
        {
            var user = new ApplicationUser
            {
                UserName = signupDto.UserName,
                Email = signupDto.Email,
                Name = signupDto.Name,
                Surname = signupDto.Surname
            };

            var result = await _userManager.CreateAsync(user, signupDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                return BadRequest(new { Errors = errors });
            }

            return NoContent();
        }

        [AllowAnonymous] // TODO: JWT çalışınca kaldır
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            // Try to get from JWT claim first
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);
            
            ApplicationUser? user = null;
            
            if (userIdClaim != null)
            {
                user = await _userManager.FindByIdAsync(userIdClaim.Value);
            }
            else
            {
                // Fallback: try email claim for testing
                var emailClaim = User.Claims.FirstOrDefault(x => x.Type == "email");
                if (emailClaim != null)
                {
                    user = await _userManager.FindByEmailAsync(emailClaim.Value);
                }
            }

            if (user == null)
            {
                // For testing only - return mock data
                return Ok(new { Id = "test", UserName = "Test User", Email = "test@test.com", Name = "Test", Surname = "User" });
            }

            return Ok(new { Id = user.Id, UserName = user.UserName, Email = user.Email, Name = user.Name, Surname = user.Surname });
        }
    }
}
