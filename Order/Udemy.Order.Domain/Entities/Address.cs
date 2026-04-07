namespace Udemy.Order.Domain.Entities
{
    /// <summary>
    /// Sipariş adresi - DDD'de ValueObject olarak tanımlı, saf Onion'da Entity olarak
    /// </summary>
    public class Address
    {
        public int Id { get; set; }
        public string Province { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public string Line { get; set; } = null!;

        // Navigation property
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
