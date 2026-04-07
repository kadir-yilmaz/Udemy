using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Udemy.NewIdentityServer.Dtos;
using Udemy.NewIdentityServer.Models;

namespace Udemy.NewIdentityServer.Controllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
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
            try
            {
                var user = new ApplicationUser
                {
                    UserName = signupDto.UserName,
                    Email = signupDto.Email,
                    City = null, // Explicitly null
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
            catch (Exception ex)
            {
                Console.WriteLine($"[SignUp Error] {ex}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

            ApplicationUser? user = null;

            if (userIdClaim != null)
            {
                user = await _userManager.FindByIdAsync(userIdClaim.Value);
            }
            else
            {
                var emailClaim = User.Claims.FirstOrDefault(x => x.Type == "email");
                if (emailClaim != null)
                {
                    user = await _userManager.FindByEmailAsync(emailClaim.Value);
                }
            }

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { Id = user.Id, UserName = user.UserName, Email = user.Email, Name = user.Name, Surname = user.Surname });
        }
    }
}
