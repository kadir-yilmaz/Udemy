using Udemy.WebUI.Models;
using Udemy.WebUI.Models.Discounts;
using Udemy.WebUI.Services.Abstract;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Concrete
{
    public class DiscountService : IDiscountService
    {
        private readonly HttpClient _httpClient;

        public DiscountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CreateDiscount(DiscountCreateInput discountCreateInput)
        {
            var discountDto = new 
            {
                Code = discountCreateInput.Code,
                Rate = discountCreateInput.Rate,
                UserId = discountCreateInput.UserId,
                ExpirationDate = discountCreateInput.ExpirationDate,
                AllowedCourseIds = discountCreateInput.AllowedCourseIds != null 
                    ? string.Join(",", discountCreateInput.AllowedCourseIds) 
                    : null
            };

            var response = await _httpClient.PostAsJsonAsync("discounts", discountDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDiscount(int id)
        {
            var response = await _httpClient.DeleteAsync($"discounts/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<DiscountViewModel>> GetAllDiscounts()
        {
            // Discount API direkt liste dönüyor, Response<T> wrapper yok
            var response = await _httpClient.GetAsync("discounts");
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<DiscountViewModel>();
            }

            var discounts = await response.Content.ReadFromJsonAsync<List<DiscountViewModel>>();
            return discounts;
        }

        public async Task<List<DiscountViewModel>> GetDiscountsByUserId(string userId)
        {
            var response = await _httpClient.GetAsync($"discounts/user/{userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<DiscountViewModel>();
            }

            var discounts = await response.Content.ReadFromJsonAsync<List<DiscountViewModel>>();
            return discounts;
        }

        public async Task<DiscountViewModel> GetDiscount(string discountCode)
        {
            // Artık API'de code/{code} endpoint'i var ve userId bağımsız çalışıyor
            var response = await _httpClient.GetAsync($"discounts/code/{discountCode}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Discount API direkt model dönüyor, Response<T> wrapper yok (Controller kodundan belli: return Ok(discount))
            // Fakat diğer metodlarda Response<T> bekliyor muyum? 
            // WebUI DiscountService GetAllDiscounts metodunda: await response.Content.ReadFromJsonAsync<List<DiscountViewModel>>(); (Direkt liste)
            // Demek ki API direkt obje dönüyor.
            // Fakat eski GetDiscount kodunda: ReadFromJsonAsync<Response<DiscountViewModel>>() vardı. Bu hataydı muhtemelen.
            // API Controller return Ok(discount) yapıyor. Response<T> wrapper yok.
            
            var discount = await response.Content.ReadFromJsonAsync<DiscountViewModel>();
            return discount;
        }
    }
}
