using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Udemy.WebUI.Models;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ICatalogService _catalogService;

        public InstructorController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public async Task<IActionResult> Profile(string id, string? name = null)
        {
            var courses = await _catalogService.GetAllCourseByUserIdAsync(id);

            // Öncelik: URL'den gelen name > Kurstan gelen UserName > Default
            var userName = name ?? courses?.FirstOrDefault()?.UserName ?? "Eğitmen";

            var model = new InstructorProfileViewModel
            {
                UserId = id,
                UserName = userName,
                Title = "Yazılım Geliştirici & Eğitmen",
                Description = "Merhaba! Ben yazılım geliştirmeye tutkulu bir eğitmenim. .NET ekosistemi, Mikroservis mimarileri ve modern web teknolojileri üzerine uzmanlaşmış durumdayım. Udemy üzerinde paylaştığım kurslarla binlerce öğrenciye ulaştım. Amacım, karmaşık konuları en sade ve anlaşılır biçimde aktararak, sektörde nitelikli yazılımcıların yetişmesine katkıda bulunmak.",
                TotalStudents = new System.Random().Next(1000, 50000),
                TotalReviews = new System.Random().Next(100, 5000),
                ProfileImageUrl = "https://ui-avatars.com/api/?name=" + userName + "&background=random&size=200",
                Courses = courses ?? new System.Collections.Generic.List<Models.Catalogs.CourseViewModel>()
            };

            return View(model);
        }
    }
}
