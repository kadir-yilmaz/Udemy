using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Services.Abstract;
namespace Udemy.WebUI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public UserController(IUserService userService, IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _userService.GetUser());
        }

        public async Task<IActionResult> MyCourses()
        {
            var orders = await _orderService.GetOrder();
            
            // Extract all unique purchased courses from all orders
            var purchasedCourses = orders?
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.ProductId)
                .Select(g => g.First())
                .ToList() ?? new List<Models.Orders.OrderItemViewModel>();

            return View(purchasedCourses);
        }
    }
}
