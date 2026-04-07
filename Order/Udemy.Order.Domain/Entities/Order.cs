using Udemy.Order.Domain.Common;
using Udemy.Order.Domain.Enums;

namespace Udemy.Order.Domain.Entities
{
    /// <summary>
    /// Sipariş entity'si
    /// </summary>
    public class Order : BaseEntity
    {
        public string BuyerId { get; set; } = null!;
        
        /// <summary>
        /// Sipariş durumu (Pending, Completed, Failed)
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Navigation properties
        public Address Address { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Toplam sipariş tutarını hesaplar
        /// </summary>
        public decimal GetTotalPrice => OrderItems.Sum(x => x.Price);
    }
}
