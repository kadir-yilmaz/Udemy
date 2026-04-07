using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IUserService _userService;

        public HomeController(ICatalogService catalogService, IUserService userService)
        {
            _catalogService = catalogService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userService.GetUserId;
            var courses = await _catalogService.GetAllCourseByUserIdAsync(userId);
            
            ViewBag.TotalCourses = courses?.Count ?? 0;
            
            return View(courses);
        }
    }
}
