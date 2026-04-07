namespace Udemy.Shared.Events
{
    /// <summary>
    /// Ödeme başarısız olduğunda yayınlanan event.
    /// FakePayment.API tarafından publish edilir, Order.API tarafından consume edilir.
    /// </summary>
    public class PaymentFailed
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = null!;
    }
}
