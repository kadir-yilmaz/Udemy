using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Models.Discounts;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class DiscountsController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IUserService _userService;
        private readonly ICatalogService _catalogService;

        public DiscountsController(IDiscountService discountService, IUserService userService, ICatalogService catalogService)
        {
            _discountService = discountService;
            _userService = userService;
            _catalogService = catalogService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userService.GetUserId;
            var discounts = await _discountService.GetDiscountsByUserId(userId);
            return View(discounts);
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userService.GetUserId;
            var courses = await _catalogService.GetAllCourseByUserIdAsync(userId);
            
            ViewBag.Courses = new Microsoft.AspNetCore.Mvc.Rendering.MultiSelectList(courses, "Id", "Name");

            return View(new DiscountCreateInput { 
                UserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DiscountCreateInput discountCreateInput)
        {
            discountCreateInput.UserId = _userService.GetUserId;

            if (!ModelState.IsValid)
            {
                var courses = await _catalogService.GetAllCourseByUserIdAsync(discountCreateInput.UserId);
                ViewBag.Courses = new Microsoft.AspNetCore.Mvc.Rendering.MultiSelectList(courses, "Id", "Name");
                return View(discountCreateInput);
            }

            var result = await _discountService.CreateDiscount(discountCreateInput);

            if (result)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorCourses = await _catalogService.GetAllCourseByUserIdAsync(discountCreateInput.UserId);
            ViewBag.Courses = new Microsoft.AspNetCore.Mvc.Rendering.MultiSelectList(errorCourses, "Id", "Name");
            ModelState.AddModelError("", "İndirim kuponu oluşturulurken bir hata oluştu.");
            return View(discountCreateInput);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _discountService.DeleteDiscount(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
