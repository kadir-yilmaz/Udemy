namespace Udemy.WebUI.Models.FakePayments
{
    public class PaymentInfoInput
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
        
        // Buyer (Alıcı) Bilgileri
        public string CustomerName { get; set; } = null!;
        public string CustomerSurname { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public string CustomerIdentityNumber { get; set; } = "11111111111";
        public string CustomerIp { get; set; } = "127.0.0.1";
        
        // Adres Bilgileri
        public string AddressDetail { get; set; } = null!;
        public string City { get; set; } = "Istanbul";
        public string Country { get; set; } = "Turkey";
        public string ZipCode { get; set; } = "34000";
        
        // Ürünler
        public List<PaymentItemInput> Items { get; set; } = new();
    }

    public class PaymentItemInput
    {
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}

