using Udemy.WebUI.Models.Discounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IDiscountService
    {
        Task<DiscountViewModel> GetDiscount(string discountCode);
        Task<List<DiscountViewModel>> GetAllDiscounts();
        Task<List<DiscountViewModel>> GetDiscountsByUserId(string userId);
        Task<bool> CreateDiscount(DiscountCreateInput discountCreateInput);
        Task<bool> DeleteDiscount(int id);
    }
}
