namespace Udemy.Invoice.API.Models
{
    /// <summary>
    /// Fatura bilgileri modeli
    /// </summary>
    public class InvoiceData
    {
        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString("N").ToUpper()[..12];
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
    }

    public class InvoiceItem
    {
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}

