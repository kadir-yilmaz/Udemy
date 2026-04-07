using Udemy.WebUI.Models;
using Udemy.WebUI.Models.Baskets;
using Udemy.WebUI.Services.Abstract;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Concrete
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;
        private readonly IDiscountService _discountService;
        private readonly IUserService _userService;

        public BasketService(HttpClient httpClient, IDiscountService discountService, IUserService userService)
        {
            _httpClient = httpClient;
            _discountService = discountService;
            _userService = userService;
            
            // Guest ID Header
            // Burası her istekte çalışmaz çünkü Singleton değil Scoped/Transient olabilir ama HttpClient factory ile 
            // instance yeniden oluştuğu için constructor her seferinde çalışır.
            // ÖNEMLİ: UserService'den o anki ID'yi alıp header'a ekliyoruz.
            // Ancak HttpClient instance'ı pool'dan geliyorsa headerlar kalıcı olabilir, bu yüzden önce varsa siliyoruz.
            // (Güvenli yöntem: SendAsync içinde yapmak ama burada ctor pratik bir çözüm)
             if (_httpClient.DefaultRequestHeaders.Contains("x-user-id"))
            {
               _httpClient.DefaultRequestHeaders.Remove("x-user-id");
            }
            _httpClient.DefaultRequestHeaders.Add("x-user-id", _userService.GetUserId);
        }

        public async Task AddBasketItem(BasketItemViewModel basketItemViewModel)
        {
            var basket = await Get();

            if (basket != null)
            {
                if (!basket.BasketItems.Any(x => x.CourseId == basketItemViewModel.CourseId))
                {
                    basket.BasketItems.Add(basketItemViewModel);
                }
            }
            else
            {
                basket = new BasketViewModel();
                basket.BasketItems.Add(basketItemViewModel);
            }

            await SaveOrUpdate(basket);
        }

        public async Task<bool> ApplyDiscount(string discountCode)
        {
            await CancelApplyDiscount();

            var basket = await Get();
            if (basket == null)
            {
                return false;
            }

            var hasDiscount = await _discountService.GetDiscount(discountCode);
            if (hasDiscount == null)
            {
                return false;
            }

            // Expiration Check
            if (hasDiscount.ExpirationDate.HasValue && hasDiscount.ExpirationDate.Value < DateTime.Now)
            {
                return false; // Expired
            }

            // Populate Allowed Courses
            List<string>? allowedCourseIds = null;
            if (!string.IsNullOrEmpty(hasDiscount.AllowedCourseIds))
            {
                allowedCourseIds = hasDiscount.AllowedCourseIds.Split(',').ToList();
            }

            // Check if any basket item is eligible for this discount
            // If discount has allowed courses but none of them are in the basket, reject the coupon
            if (allowedCourseIds != null && allowedCourseIds.Any())
            {
                var hasEligibleItem = basket.BasketItems.Any(item => allowedCourseIds.Contains(item.CourseId));
                if (!hasEligibleItem)
                {
                    return false; // Coupon exists but doesn't apply to any item in basket
                }
            }

            basket.AllowedCourseIds = allowedCourseIds;
            basket.ApplyDiscount(hasDiscount.Code, hasDiscount.Rate);
            await SaveOrUpdate(basket);
            return true;
        }

        public async Task<bool> CancelApplyDiscount()
        {
            var basket = await Get();

            if (basket == null || basket.DiscountCode == null)
            {
                return false;
            }

            basket.CancelDiscount();
            await SaveOrUpdate(basket);
            return true;
        }

        public async Task<bool> Delete()
        {
            var result = await _httpClient.DeleteAsync("baskets");
            return result.IsSuccessStatusCode;
        }

        public async Task<BasketViewModel?> Get()
        {
            var response = await _httpClient.GetAsync("baskets");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var basketViewModel = await response.Content.ReadFromJsonAsync<BasketViewModel>();

            // Re-validate and Re-populate Discount Logic
            if (basketViewModel != null && !string.IsNullOrEmpty(basketViewModel.DiscountCode))
            {
                var discount = await _discountService.GetDiscount(basketViewModel.DiscountCode);
                
                // If discount not found or Expired, cancel it
                if (discount == null || (discount.ExpirationDate.HasValue && discount.ExpirationDate.Value < DateTime.Now))
                {
                    basketViewModel.CancelDiscount();
                    // We should probably save this cancellation to backend, but for "Get" (Read) usually we just display clean state
                    // or ideally update backend. Let's just return clean viewmodel for now.
                    basketViewModel.AllowedCourseIds = null;
                }
                else
                {
                    // Populate Allowed Courses for UI Calculation
                    if (!string.IsNullOrEmpty(discount.AllowedCourseIds))
                    {
                        basketViewModel.AllowedCourseIds = discount.AllowedCourseIds.Split(',').ToList();
                    }
                }
            }

            return basketViewModel;
        }

        public async Task<bool> RemoveBasketItem(string courseId)
        {
            var basket = await Get();

            if (basket == null)
            {
                return false;
            }

            var deleteBasketItem = basket.BasketItems.FirstOrDefault(x => x.CourseId == courseId);

            if (deleteBasketItem == null)
            {
                return false;
            }

            var deleteResult = basket.BasketItems.Remove(deleteBasketItem);

            if (!deleteResult)
            {
                return false;
            }

            if (!basket.BasketItems.Any())
            {
                basket.DiscountCode = null;
            }

            return await SaveOrUpdate(basket);
        }

        public async Task<bool> SaveOrUpdate(BasketViewModel basketViewModel)
        {
            var basketInput = new BasketInput
            {
                UserId = basketViewModel.UserId,
                Email = _userService.GetUserEmail, // Email'i UserService'den al
                DiscountCode = basketViewModel.DiscountCode,
                DiscountRate = basketViewModel.DiscountRate,
                BasketItems = basketViewModel.BasketItems.Select(x => new BasketItemInput
                {
                    CourseId = x.CourseId,
                    CourseName = x.CourseName,
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync<BasketInput>("baskets", basketInput);
            return response.IsSuccessStatusCode;
        }

        public async Task TransferBasket(string userId, string email)
        {
            // API'deki transfer endpointini çağır
            var transferInput = new { UserId = userId, Email = email };
            await _httpClient.PostAsJsonAsync("baskets/transfer", transferInput);
        }
    }
}
