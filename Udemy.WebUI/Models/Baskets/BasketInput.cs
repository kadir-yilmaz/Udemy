using System.Collections.Generic;

namespace Udemy.WebUI.Models.Baskets
{
    public class BasketInput
    {
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountRate { get; set; }
        public List<BasketItemInput> BasketItems { get; set; } = new List<BasketItemInput>();
    }

    public class BasketItemInput
    {
        public int Quantity { get; set; }
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal Price { get; set; }
    }
}
