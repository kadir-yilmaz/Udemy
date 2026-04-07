using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Models.Orders;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public OrderController(IBasketService basketService, IOrderService orderService)
        {
            _basketService = basketService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Checkout()
        {
            await PopulateCheckoutViewAsync();
            return View(new CheckoutInfoInput());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutInfoInput checkoutInfoInput)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCheckoutViewAsync();
                return View(checkoutInfoInput);
            }

            var orderSuspend = await _orderService.SuspendOrder(checkoutInfoInput);

            if (!orderSuspend.IsSuccessful)
            {
                await PopulateCheckoutViewAsync();
                ViewBag.error = orderSuspend.Error;
                return View(checkoutInfoInput);
            }

            var orderId = new Random().Next(1, 1000);
            TempData["OrderSuccess"] = true;
            TempData["OrderId"] = orderId;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult SuccessfulCheckout(int orderId)
        {
            ViewBag.orderId = orderId;
            return View();
        }

        public async Task<IActionResult> CheckoutHistory()
        {
            return View(await _orderService.GetOrder());
        }

        private async Task PopulateCheckoutViewAsync()
        {
            var basket = await _basketService.Get();
            ViewBag.basket = basket;

            ViewBag.ExpireMonths = Enumerable.Range(1, 12)
                .Select(x => x.ToString("00"))
                .ToList();

            var currentYear = DateTime.UtcNow.Year % 100;
            ViewBag.ExpireYears = Enumerable.Range(currentYear, 10)
                .Select(x => x.ToString("00"))
                .ToList();
        }
    }
}
