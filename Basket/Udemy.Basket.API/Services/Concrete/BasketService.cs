using System.Text.Json;
using Udemy.Basket.API.Dtos;
using Udemy.Basket.API.Services.Abstract;

namespace Udemy.Basket.API.Services.Concrete
{
    public class BasketService : IBasketService
    {
        private readonly RedisService _redisService;

        public BasketService(RedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<BasketDto> GetBasket(string userId)
        {
            var existBasket = await _redisService
                .GetDb()
                .StringGetAsync(userId);

            if (string.IsNullOrEmpty(existBasket))
            {
                return null;
            }

            return JsonSerializer.Deserialize<BasketDto>(existBasket);
        }

        public async Task<bool> SaveOrUpdate(BasketDto basketDto)
        {
            var status = await _redisService
                .GetDb()
                .StringSetAsync(
                    basketDto.UserId,
                    JsonSerializer.Serialize(basketDto)
                );

            return status;
        }

        public async Task<bool> Delete(string userId)
        {
            return await _redisService
                .GetDb()
                .KeyDeleteAsync(userId);
        }
    }
}
