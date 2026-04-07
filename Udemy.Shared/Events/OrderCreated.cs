namespace Udemy.Shared.Events
{
    /// <summary>
    /// Sipariş oluşturulduğunda yayınlanan event.
    /// Order.API tarafından publish edilir, FakePayment.API tarafından consume edilir.
    /// </summary>
    public class OrderCreated
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        
        // Adres bilgileri
        public string Province { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string Line { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        
        // Sipariş kalemleri
        public List<OrderItemMessage> OrderItems { get; set; } = new();
    }

    public class OrderItemMessage
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string PictureUrl { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
