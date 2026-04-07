using Udemy.WebUI.Models.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IOrderService
    {
        Task<OrderCreatedViewModel> CreateOrder(CheckoutInfoInput checkoutInfoInput);
        Task<OrderSuspendViewModel> SuspendOrder(CheckoutInfoInput checkoutInfoInput);
        Task<List<OrderViewModel>> GetOrder();
        Task<List<string>> GetOwnedCourseIds();
        Task<CourseOwnershipResult> CheckCourseOwnership(string courseId);
    }

    public class CourseOwnershipResult
    {
        public bool Owns { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}

