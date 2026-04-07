using Udemy.Basket.API.Dtos;

namespace Udemy.Basket.API.Services.Abstract
{
    public interface IBasketService
    {
        Task<BasketDto> GetBasket(string userId);
        Task<bool> SaveOrUpdate(BasketDto basketDto);
        Task<bool> Delete(string userId);
    }
}
