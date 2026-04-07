using Udemy.WebUI.Models.Baskets;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IBasketService
    {
        Task<bool> SaveOrUpdate(BasketViewModel basketViewModel);

        Task<BasketViewModel?> Get();

        Task<bool> Delete();

        Task AddBasketItem(BasketItemViewModel basketItemViewModel);

        Task<bool> RemoveBasketItem(string courseId);

        Task<bool> ApplyDiscount(string discountCode);

        Task<bool> CancelApplyDiscount();

        Task TransferBasket(string userId, string email);
    }
}
