namespace Udemy.Shared.Events
{
    /// <summary>
    /// Ödeme başarılı olduğunda fatura emaili göndermek için yayınlanan event.
    /// FakePayment.API tarafından publish edilir, Invoice.API tarafından consume edilir.
    /// </summary>
    public class InvoiceRequested
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public List<InvoiceItemMessage> Items { get; set; } = new();
    }

    public class InvoiceItemMessage
    {
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
