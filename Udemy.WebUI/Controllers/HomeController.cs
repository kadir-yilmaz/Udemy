using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Udemy.WebUI.Exceptions;
using Udemy.WebUI.Models;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IOrderService _orderService;

        public HomeController(
            ILogger<HomeController> logger, 
            ICatalogService catalogService,
            IOrderService orderService)
        {
            _logger = logger;
            _catalogService = catalogService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _catalogService.GetAllCourseAsync();
            var ownedCourseIds = await _orderService.GetOwnedCourseIds();
            ViewBag.OwnedCourseIds = ownedCourseIds;
            
            return View(courses);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var course = await _catalogService.GetByCourseId(id);
            
            if (course == null)
            {
                return RedirectToAction(nameof(Index));
            }
            
            // Kullanıcı bu kursa sahip mi kontrol et
            var ownership = await _orderService.CheckCourseOwnership(id);
            ViewBag.IsOwned = ownership.Owns;
            ViewBag.PurchaseDate = ownership.PurchaseDate;
            
            return View(course);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (errorFeature != null && errorFeature.Error is UnAuthorizeException)
            {
                return RedirectToAction(nameof(AuthController.Logout), "Auth");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

