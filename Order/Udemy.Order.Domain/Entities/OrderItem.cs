using Udemy.Order.Domain.Common;

namespace Udemy.Order.Domain.Entities
{
    /// <summary>
    /// Sipariş kalemi
    /// </summary>
    public class OrderItem : BaseEntity
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string PictureUrl { get; set; } = null!;
        public decimal Price { get; set; }

        // Navigation property
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
