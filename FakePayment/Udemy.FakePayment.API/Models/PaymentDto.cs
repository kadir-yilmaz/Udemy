namespace Udemy.FakePayment.API.Models
{
    /// <summary>
    /// Ödeme isteği modeli - iyzico entegrasyonu için genişletildi
    /// </summary>
    public class PaymentDto
    {
        // Kart Bilgileri
        public string CardName { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public string ExpireMonth { get; set; } = null!;
        public string ExpireYear { get; set; } = null!;
        public string CVV { get; set; } = null!;
        
        // Sipariş Bilgileri
        public decimal TotalPrice { get; set; }
        public int OrderId { get; set; }
        
        // Buyer (Alıcı) Bilgileri - iyzico için zorunlu
        public string CustomerName { get; set; } = null!;
        public string CustomerSurname { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string CustomerIdentityNumber { get; set; } = "11111111111"; // TC Kimlik - varsayılan
        public string CustomerIp { get; set; } = "127.0.0.1";
        
        // Adres Bilgileri - iyzico için zorunlu
        public string AddressDetail { get; set; } = null!;
        public string City { get; set; } = "Istanbul";
        public string Country { get; set; } = "Turkey";
        public string ZipCode { get; set; } = "34000";
        
        // Ürünler
        public List<PaymentItemDto> Items { get; set; } = new();
    }

    public class PaymentItemDto
    {
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}



