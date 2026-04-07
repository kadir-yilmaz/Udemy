using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Udemy.Basket.API.Dtos;
using Udemy.Basket.API.Services.Abstract;

namespace Udemy.Basket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketsController(IBasketService basketService)
        {
            _basketService = basketService;
        }

        // User Token (sub) varsa onu kullan, yoksa (Client Token) Header (x-user-id) kontrol et.
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) 
                                 ?? Request.Headers["x-user-id"].FirstOrDefault();

        // GET api/baskets
        [HttpGet]
        public async Task<IActionResult> GetBasket()
        {
            var basket = await _basketService.GetBasket(UserId);

            return Ok(basket ?? new BasketDto { UserId = UserId });
        }

        // POST api/baskets
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateBasket([FromBody] BasketDto basketDto)
        {
            basketDto.UserId = UserId;
            var result = await _basketService.SaveOrUpdate(basketDto);

            return Ok(result);
        }

        // DELETE api/baskets
        [HttpDelete]
        public async Task<IActionResult> DeleteBasket()
        {
            var result = await _basketService.Delete(UserId);
            return Ok(result);
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferBasket([FromBody] TransferBasketDto transferDto)
        {
             // 1. Misafir Sepetini Çek (Header'daki ID)
            var guestBasket = await _basketService.GetBasket(UserId);

            if (guestBasket == null)
            {
                return Ok(); // Transfer edilecek sepet yok
            }

            // 2. Hedef Kullanıcı Sepetini Çek
            var userBasket = await _basketService.GetBasket(transferDto.UserId);

            if (userBasket == null)
            {
                userBasket = new BasketDto { UserId = transferDto.UserId };
            }

            // Email Ata
            userBasket.Email = transferDto.Email;

            // 3. Birleştir (Merge) - Duplike ürünleri ekleme
            foreach (var item in guestBasket.BasketItems)
            {
                if (!userBasket.BasketItems.Any(x => x.CourseId == item.CourseId))
                {
                    userBasket.BasketItems.Add(item);
                }
            }

            // 4. İndirim kodu varsa taşı (opsiyonel, şimdilik basit tutalım)
            if (string.IsNullOrEmpty(userBasket.DiscountCode) && !string.IsNullOrEmpty(guestBasket.DiscountCode))
            {
                userBasket.DiscountCode = guestBasket.DiscountCode;
                userBasket.DiscountRate = guestBasket.DiscountRate;
            }

            // 5. Hedef Sepeti Kaydet
            await _basketService.SaveOrUpdate(userBasket);

            // 6. Misafir Sepetini Sil
            await _basketService.Delete(UserId);

            return Ok();
        }
    }

    public class TransferBasketDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}

