using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Udemy.WebUI.Models.Catalogs;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IUserService _userService;

        public CoursesController(ICatalogService catalogService, IUserService userService)
        {
            _catalogService = catalogService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _catalogService.GetAllCourseByUserIdAsync(_userService.GetUserId));
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _catalogService.GetAllCategoryAsync();
            // Sort: Yazılım first, then others alphabetically
            categories = categories?.OrderBy(x => x.Name != "Yazılım").ThenBy(x => x.Name).ToList();

            var selectedId = categories?.FirstOrDefault(x => x.Name == "Yazılım")?.Id;
            ViewBag.categoryList = new SelectList(categories ?? new List<CategoryViewModel>(), "Id", "Name", selectedId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateInput courseCreateInput)
        {
            var categories = await _catalogService.GetAllCategoryAsync();
            categories = categories?.OrderBy(x => x.Name != "Yazılım").ThenBy(x => x.Name).ToList();
            ViewBag.categoryList = new SelectList(categories ?? new List<CategoryViewModel>(), "Id", "Name");

            if (!ModelState.IsValid)
            {
                return View(courseCreateInput);
            }

            courseCreateInput.UserId = _userService.GetUserId;
            var result = await _catalogService.CreateCourseAsync(courseCreateInput);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Kurs oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(courseCreateInput);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(string id)
        {
            var course = await _catalogService.GetByCourseId(id);
            var categories = await _catalogService.GetAllCategoryAsync();
            categories = categories?.OrderBy(x => x.Name != "Yazılım").ThenBy(x => x.Name).ToList();

            if (course == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.categoryList = new SelectList(categories, "Id", "Name", course.Id);

            CourseUpdateInput courseUpdateInput = new()
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                Feature = course.Feature,
                CategoryId = course.CategoryId,
                UserId = course.UserId,
                Picture = course.Picture
            };

            return View(courseUpdateInput);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CourseUpdateInput courseUpdateInput)
        {
            var categories = await _catalogService.GetAllCategoryAsync();
            categories = categories?.OrderBy(x => x.Name != "Yazılım").ThenBy(x => x.Name).ToList();
            ViewBag.categoryList = new SelectList(categories, "Id", "Name", courseUpdateInput.Id);

            if (!ModelState.IsValid)
            {
                return View(courseUpdateInput);
            }

            var result = await _catalogService.UpdateCourseAsync(courseUpdateInput);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Kurs güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(courseUpdateInput);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _catalogService.DeleteCourseAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
