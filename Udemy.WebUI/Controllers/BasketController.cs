using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Models.Baskets;
using Udemy.WebUI.Models.Discounts;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    // [Authorize] - Misafir sepeti için kaldırıldı
    public class BasketController : Controller
    {
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public BasketController(
            ICatalogService catalogService, 
            IBasketService basketService,
            IOrderService orderService)
        {
            _catalogService = catalogService;
            _basketService = basketService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _basketService.Get());
        }

        public async Task<IActionResult> AddBasketItem(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return RedirectToAction("Index", "Home");
            }
            
            var course = await _catalogService.GetByCourseId(courseId);
            
            if (course == null)
            {
                TempData["Error"] = "Kurs bulunamadı veya artık mevcut değil.";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcı bu kursu daha önce satın almış mı?
            var ownership = await _orderService.CheckCourseOwnership(courseId);
            if (ownership.Owns)
            {
                TempData["Error"] = $"Bu kursu zaten satın aldınız. ({ownership.PurchaseDate:dd.MM.yyyy})";
                return RedirectToAction("Detail", "Course", new { id = courseId });
            }

            var basketItem = new BasketItemViewModel 
            { 
                CourseId = course.Id, 
                CourseName = course.Name, 
                Price = course.Price,
                PictureUrl = course.StockPictureUrl
            };

            await _basketService.AddBasketItem(basketItem);

            TempData["BasketNotification"] = "true";

            // Return to previous page if available
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemoveBasketItem(string courseId)
        {
            await _basketService.RemoveBasketItem(courseId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ApplyDiscount(DiscountApplyInput discountApplyInput)
        {
            if (!ModelState.IsValid)
            {
                TempData["discountError"] = ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .First();
                return RedirectToAction(nameof(Index));
            }

            var discountStatus = await _basketService.ApplyDiscount(discountApplyInput.Code);
            TempData["discountStatus"] = discountStatus;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CancelApplyDiscount()
        {
            await _basketService.CancelApplyDiscount();
            return RedirectToAction(nameof(Index));
        }
    }
}
