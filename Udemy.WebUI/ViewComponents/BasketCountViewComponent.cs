using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.ViewComponents
{
    public class BasketCountViewComponent : ViewComponent
    {
        private readonly IBasketService _basketService;

        public BasketCountViewComponent(IBasketService basketService)
        {
            _basketService = basketService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var basket = await _basketService.Get();
            var count = basket?.BasketItems.Count ?? 0;
            return View(count);
        }
    }
}
